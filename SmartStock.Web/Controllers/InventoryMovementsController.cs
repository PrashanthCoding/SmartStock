
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Application.DTOs.InventoryMovements;
using SmartStock.Domain.Entities;
using SmartStock.Domain.Enums;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMovementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventoryMovementsController> _logger;

        public InventoryMovementsController(
            ApplicationDbContext context,
            ILogger<InventoryMovementsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================
        // GET: api/InventoryMovements
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var movements = await _context.InventoryMovements
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new
                {
                    x.Id,
                    x.InventoryId,
                    x.ProductId,
                    ProductName = x.Product.ProductName,
                    x.MovementType,
                    x.Quantity,
                    x.QuantityBefore,
                    x.QuantityAfter,
                    x.Remarks,
                    x.CreatedAtUtc
                })
                .ToListAsync();

            return Ok(movements);
        }

        // =====================================================
        // GET: api/InventoryMovements/5
        // =====================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Valid movement ID is required."
                });

            var movement = await _context.InventoryMovements
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.InventoryId,
                    x.ProductId,
                    ProductName = x.Product.ProductName,
                    x.MovementType,
                    x.Quantity,
                    x.QuantityBefore,
                    x.QuantityAfter,
                    x.Remarks,
                    x.CreatedAtUtc
                })
                .FirstOrDefaultAsync();

            if (movement == null)
            {
                return NotFound(new
                {
                    message = "Inventory movement not found."
                });
            }

            return Ok(movement);
        }

        // =====================================================
        // POST: api/InventoryMovements
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateInventoryMovementDto dto)
        {
            // -------------------------------------------------
            // 1. VALIDATION
            // -------------------------------------------------

            if (dto == null)
            {
                return BadRequest(new
                {
                    message = "Request body is required."
                });
            }

            if (dto.ProductId <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid ProductId is required."
                });
            }

            if (dto.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message = "Quantity must be greater than zero."
                });
            }

            if (!Enum.IsDefined(typeof(InventoryMovementType), dto.MovementType))
            {
                return BadRequest(new
                {
                    message = "Invalid movement type.",
                    receivedValue = (int)dto.MovementType,
                    allowedValues = new[]
                    {
                        (int)InventoryMovementType.StockIn,
                        (int)InventoryMovementType.StockOut,
                        (int)InventoryMovementType.Adjustment
                    }
                });
            }

            if (dto.RowVersion == null || dto.RowVersion.Length == 0)
            {
                return BadRequest(new
                {
                    message = "RowVersion is required. Please refresh inventory and try again."
                });
            }

            if (dto.Remarks?.Length > 500)
            {
                return BadRequest(new
                {
                    message = "Remarks cannot exceed 500 characters."
                });
            }

            _logger.LogInformation(
                "Received inventory movement. ProductId: {ProductId}, MovementType: {MovementType}, NumericValue: {Value}",
                dto.ProductId,
                dto.MovementType,
                (int)dto.MovementType);

            // -------------------------------------------------
            // 2. DATABASE TRANSACTION
            // -------------------------------------------------

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // -------------------------------------------------
                // 3. GET INVENTORY
                // -------------------------------------------------

                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

                if (inventory == null)
                {
                    await transaction.RollbackAsync();

                    return NotFound(new
                    {
                        message = "Inventory not found for this product."
                    });
                }

                // -------------------------------------------------
                // 4. ROW VERSION CONCURRENCY VALIDATION
                // -------------------------------------------------

                _context.Entry(inventory)
                    .Property(x => x.RowVersion)
                    .OriginalValue = dto.RowVersion;

                var quantityBefore = inventory.Quantity;
                decimal quantityAfter;

                // -------------------------------------------------
                // 5. CALCULATE STOCK
                // -------------------------------------------------

                switch (dto.MovementType)
                {
                    case InventoryMovementType.StockIn:

                        quantityAfter = quantityBefore + dto.Quantity;
                        break;

                    case InventoryMovementType.StockOut:

                        if (quantityBefore < dto.Quantity)
                        {
                            await transaction.RollbackAsync();

                            return BadRequest(new
                            {
                                message = "Insufficient stock available.",
                                availableQuantity = quantityBefore,
                                requestedQuantity = dto.Quantity
                            });
                        }

                        quantityAfter = quantityBefore - dto.Quantity;
                        break;

                    case InventoryMovementType.Adjustment:

                        // Quantity represents the actual physical stock count.
                        quantityAfter = dto.Quantity;
                        break;

                    default:

                        await transaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message = "Invalid movement type."
                        });
                }

                // -------------------------------------------------
                // 6. CALCULATE MOVEMENT QUANTITY
                // -------------------------------------------------

                var movementQuantity =
                    dto.MovementType == InventoryMovementType.Adjustment
                        ? Math.Abs(quantityAfter - quantityBefore)
                        : dto.Quantity;

                var now = DateTime.UtcNow;

                // -------------------------------------------------
                // 7. UPDATE INVENTORY
                // -------------------------------------------------

                inventory.Quantity = quantityAfter;
                inventory.UpdatedAtUtc = now;

                // -------------------------------------------------
                // 8. CREATE INVENTORY MOVEMENT
                // -------------------------------------------------

                var movement = new InventoryMovement
                {
                    InventoryId = inventory.Id,
                    ProductId = inventory.ProductId,
                    MovementType = dto.MovementType,
                    Quantity = movementQuantity,
                    QuantityBefore = quantityBefore,
                    QuantityAfter = quantityAfter,
                    Remarks = dto.Remarks?.Trim(),
                    CreatedAtUtc = now
                };

                _context.InventoryMovements.Add(movement);

                // -------------------------------------------------
                // 9. SAVE BOTH CHANGES
                // -------------------------------------------------

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // -------------------------------------------------
                // 10. LOG SUCCESS
                // -------------------------------------------------

                _logger.LogInformation(
                    "Inventory movement {MovementId} created for Product {ProductId}. Type: {MovementType}, Before: {Before}, After: {After}",
                    movement.Id,
                    inventory.ProductId,
                    movement.MovementType,
                    quantityBefore,
                    quantityAfter);

                // -------------------------------------------------
                // 11. RESPONSE
                // -------------------------------------------------

                return Ok(new
                {
                    message = "Inventory movement recorded successfully.",
                    movementId = movement.Id,
                    inventoryId = inventory.Id,
                    productId = inventory.ProductId,
                    movementType = movement.MovementType,
                    quantityBefore,
                    quantityMoved = movementQuantity,
                    quantityAfter,
                    rowVersion = Convert.ToBase64String(inventory.RowVersion)
                });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();

                _logger.LogWarning(
                    ex,
                    "Concurrency conflict while updating inventory for Product {ProductId}",
                    dto.ProductId);

                return Conflict(new
                {
                    message = "Inventory was modified by another user. Please refresh and try again."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error creating inventory movement for Product {ProductId}",
                    dto.ProductId);

                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while recording inventory movement."
                });
            }
        }
    }
}
