using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Application.DTOs.CustomerOrders;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET: api/CustomerOrders
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.CustomerOrders
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.OrderDateUtc)
                .ToListAsync();

            return Ok(orders.Select(MapOrder));
        }

        // =========================================
        // GET: api/CustomerOrders/5
        // =========================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.CustomerOrders
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            return Ok(MapOrder(order));
        }

        // =========================================
        // POST: api/CustomerOrders
        // Create order and deduct stock
        // =========================================
        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateCustomerOrderDto dto)
        {
            if (dto.CustomerId <= 0)
                return BadRequest(new { message = "Valid customer is required." });

            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest(new { message = "At least one order item is required." });

            if (dto.Items.Any(x => x.ProductId <= 0 || x.Quantity <= 0))
                return BadRequest(new
                {
                    message = "Each product must have a valid ID and quantity greater than zero."
                });

            // Combine repeated product IDs to avoid duplicate stock processing.
            var requestedItems = dto.Items
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                // 1. Validate customer
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CustomerId && x.IsActive);

                if (customer == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "Customer not found or inactive."
                    });
                }

                var productIds = requestedItems
                    .Select(x => x.ProductId)
                    .ToList();

                // 2. Load active products
                var products = await _context.Products
                    .Where(x => productIds.Contains(x.Id) && x.IsActive)
                    .ToListAsync();

                if (products.Count != productIds.Count)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "One or more products do not exist or are inactive."
                    });
                }

                // 3. Load inventory records
                var inventories = await _context.Inventories
                    .Where(x => productIds.Contains(x.ProductId))
                    .ToListAsync();

                if (inventories.Count != productIds.Count)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "Inventory is missing for one or more products."
                    });
                }

                var productLookup = products.ToDictionary(x => x.Id);
                var inventoryLookup = inventories.ToDictionary(x => x.ProductId);

                // 4. Validate stock for every item before changing anything
                foreach (var item in requestedItems)
                {
                    var inventory = inventoryLookup[item.ProductId];

                    if (inventory.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message = $"Insufficient stock for product: {productLookup[item.ProductId].ProductName}.",
                            availableQuantity = inventory.Quantity,
                            requestedQuantity = item.Quantity
                        });
                    }
                }

                // 5. Create order
                var now = DateTime.UtcNow;

                var order = new CustomerOrder
                {
                    OrderNumber = $"ORD-{now:yyyyMMdd}-{Guid.NewGuid():N}"
                        .Substring(0, 21).ToUpperInvariant(),
                    CustomerId = customer.Id,
                    Customer = customer,
                    OrderDateUtc = now,
                    Status = OrderStatus.Confirmed,
                    CreatedAtUtc = now
                };

                decimal totalAmount = 0;

                // 6. Create order items and deduct stock
                foreach (var item in requestedItems)
                {
                    var product = productLookup[item.ProductId];
                    var inventory = inventoryLookup[item.ProductId];

                    var quantityBefore = inventory.Quantity;
                    var quantityAfter = quantityBefore - item.Quantity;

                    var lineTotal = item.Quantity * product.UnitPrice;
                    totalAmount += lineTotal;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Product = product,
                        Quantity = item.Quantity,
                        UnitPrice = product.UnitPrice,
                        LineTotal = lineTotal,
                        CreatedAtUtc = now
                    });

                    inventory.Quantity = quantityAfter;
                    inventory.UpdatedAtUtc = now;

                    // Record stock deduction
                    _context.InventoryMovements.Add(
                        new InventoryMovement
                        {
                            InventoryId = inventory.Id,
                            Inventory = inventory,
                            ProductId = product.Id,
                            Product = product,
                            MovementType = InventoryMovementType.StockOut,
                            Quantity = item.Quantity,
                            QuantityBefore = quantityBefore,
                            QuantityAfter = quantityAfter,
                            Remarks = $"Stock deducted for order {order.OrderNumber}",
                            CreatedAtUtc = now
                        });
                }

                order.TotalAmount = totalAmount;

                _context.CustomerOrders.Add(order);

                // 7. Save order, items, inventory and movements together
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = order.Id },
                    MapOrder(order));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                return Conflict(new
                {
                    message = "Inventory was changed by another transaction. Please refresh stock and retry."
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "An error occurred while creating the order."
                });
            }
        }

        // =========================================
        // PUT: api/CustomerOrders/5/cancel
        // Cancel order and restore stock
        // =========================================
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                // Load order and its items
                var order = await _context.CustomerOrders
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (order == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { message = "Order not found." });
                }

                // Prevent restoring stock more than once
                if (order.Status == OrderStatus.Cancelled)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new
                    {
                        message = "Order is already cancelled."
                    });
                }

                if (order.Status != OrderStatus.Confirmed)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        message = "Only confirmed orders can be cancelled."
                    });
                }

                var productIds = order.Items
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();

                var inventories = await _context.Inventories
                    .Where(x => productIds.Contains(x.ProductId))
                    .ToListAsync();

                if (inventories.Count != productIds.Count)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new
                    {
                        message = "Inventory is missing for one or more order products."
                    });
                }

                var inventoryLookup = inventories.ToDictionary(x => x.ProductId);
                var now = DateTime.UtcNow;

                // Restore stock and record StockIn movements
                foreach (var item in order.Items)
                {
                    var inventory = inventoryLookup[item.ProductId];

                    var quantityBefore = inventory.Quantity;
                    var quantityAfter = quantityBefore + item.Quantity;

                    inventory.Quantity = quantityAfter;
                    inventory.UpdatedAtUtc = now;

                    _context.InventoryMovements.Add(
                        new InventoryMovement
                        {
                            InventoryId = inventory.Id,
                            Inventory = inventory,
                            ProductId = item.ProductId,
                            MovementType = InventoryMovementType.StockIn,
                            Quantity = item.Quantity,
                            QuantityBefore = quantityBefore,
                            QuantityAfter = quantityAfter,
                            Remarks = $"Stock restored for cancelled order {order.OrderNumber}",
                            CreatedAtUtc = now
                        });
                }

                // Change status in the same transaction
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAtUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Order cancelled and stock restored successfully.",
                    order.Id,
                    order.OrderNumber,
                    order.Status
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();

                return Conflict(new
                {
                    message = "Data was changed by another transaction. Please retry."
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "An error occurred while cancelling the order."
                });
            }
        }

        // =========================================
        // Map entity to response DTO
        // =========================================
        private static CustomerOrderResponseDto MapOrder(
            CustomerOrder order)
        {
            return new CustomerOrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.CustomerName,
                OrderDateUtc = order.OrderDateUtc,
                Status = order.Status,
                TotalAmount = order.TotalAmount,

                Items = order.Items.Select(item =>
                    new OrderItemResponseDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        SKU = item.Product.SKU,
                        ProductName = item.Product.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    }).ToList()
            };
        }
    }
}
