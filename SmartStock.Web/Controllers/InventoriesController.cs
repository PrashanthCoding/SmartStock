using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Inventories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var inventories = await _context.Inventories
                .AsNoTracking()
                .Include(x => x.Product)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductName = x.Product.ProductName,
                    x.Quantity,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc
                })
                .ToListAsync();

            return Ok(inventories);
        }

        // GET: api/Inventories/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var inventory = await _context.Inventories
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductName = x.Product.ProductName,
                    x.Quantity,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc
                })
                .FirstOrDefaultAsync();

            if (inventory == null)
                return NotFound(new
                {
                    message = "Inventory not found."
                });

            return Ok(inventory);
        }

        // POST: api/Inventories
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] Inventory inventory)
        {
            if (inventory.ProductId <= 0)
                return BadRequest("Valid ProductId is required.");

            var productExists = await _context.Products
                .AnyAsync(x => x.Id == inventory.ProductId);

            if (!productExists)
                return BadRequest("Product not found.");

            var alreadyExists = await _context.Inventories
                .AnyAsync(x => x.ProductId == inventory.ProductId);

            if (alreadyExists)
                return Conflict("Inventory already exists for this product.");

            var newInventory = new Inventory
            {
                ProductId = inventory.ProductId,
                Quantity = 0,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Inventories.Add(newInventory);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = newInventory.Id },
                newInventory);
        }

        // DELETE: api/Inventories/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (inventory == null)
                return NotFound("Inventory not found.");

            var hasMovements = await _context.InventoryMovements
                .AnyAsync(x => x.InventoryId == id);

            if (hasMovements)
                return Conflict(
                    "Cannot delete inventory because stock movements exist.");

            _context.Inventories.Remove(inventory);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Inventory deleted successfully."
            });
        }



        // GET: api/Inventories/low-stock
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var lowStockItems = await _context.Inventories
                .AsNoTracking()
                .Where(x => x.Quantity <= x.Product.ReorderLevel)
                .Select(x => new
                {
                    InventoryId = x.Id,
                    x.ProductId,
                    x.Product.SKU,
                    x.Product.ProductName,
                    x.Product.Category,
                    x.Quantity,
                    x.Product.ReorderLevel,
                    ShortageQuantity = x.Product.ReorderLevel - x.Quantity,
                    StockStatus = x.Quantity == 0 ? "Out of Stock" : "Low Stock"
                })
                .OrderBy(x => x.Quantity)
                .ToListAsync();

            return Ok(lowStockItems);
        }


        // GET: api/Inventories/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
            var totalProducts = await _context.Inventories
                .Select(x => x.ProductId)
                .Distinct()
                .CountAsync();

            var totalQuantity = await _context.Inventories
                .SumAsync(x => (decimal?)x.Quantity) ?? 0;

            var lowStockCount = await _context.Inventories
                .CountAsync(x => x.Quantity <= x.Product.ReorderLevel);

            var outOfStockCount = await _context.Inventories
                .CountAsync(x => x.Quantity == 0);

            var totalInventoryValue = await _context.Inventories
                .SumAsync(x => (decimal?)(x.Quantity * x.Product.UnitPrice)) ?? 0;

            return Ok(new
            {
                TotalProducts = totalProducts,
                TotalQuantity = totalQuantity,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                TotalInventoryValue = totalInventoryValue
            });
        }
    }
}
