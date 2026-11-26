using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Application.DTOs.Customers;
using SmartStock.Domain.Entities;
using SmartStock.Infrastructure.Persistence.Context;

namespace SmartStock.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET: api/Customers
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new
                {
                    x.Id,
                    x.CustomerName,
                    x.Email,
                    x.Phone,
                    x.Address,
                    x.IsActive,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc
                })
                .ToListAsync();

            return Ok(customers);
        }

        // =========================================
        // GET: api/Customers/5
        // =========================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.CustomerName,
                    x.Email,
                    x.Phone,
                    x.Address,
                    x.IsActive,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc
                })
                .FirstOrDefaultAsync();

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            return Ok(customer);
        }

        // =========================================
        // POST: api/Customers
        // =========================================
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(
            [FromBody] CreateCustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "Customer name is required." });

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _context.Customers
                    .AnyAsync(x => x.Email == dto.Email.Trim());

                if (emailExists)
                    return Conflict(new { message = "Email already exists." });
            }

            var customer = new Customer
            {
                CustomerName = dto.CustomerName.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Address = dto.Address?.Trim(),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = customer.Id },
                new
                {
                    customer.Id,
                    customer.CustomerName,
                    customer.Email,
                    customer.Phone,
                    customer.Address,
                    customer.IsActive,
                    customer.CreatedAtUtc
                });
        }

        // =========================================
        // PUT: api/Customers/5
        // =========================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCustomer(
            int id,
            [FromBody] UpdateCustomerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "Customer name is required." });

            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _context.Customers
                    .AnyAsync(x => x.Email == dto.Email.Trim() && x.Id != id);

                if (emailExists)
                    return Conflict(new { message = "Email already exists." });
            }

            customer.CustomerName = dto.CustomerName.Trim();
            customer.Email = dto.Email?.Trim();
            customer.Phone = dto.Phone?.Trim();
            customer.Address = dto.Address?.Trim();
            customer.IsActive = dto.IsActive;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer updated successfully.",
                customer.Id,
                customer.CustomerName,
                customer.Email,
                customer.Phone,
                customer.Address,
                customer.IsActive,
                customer.UpdatedAtUtc
            });
        }

        // =========================================
        // DELETE: api/Customers/5
        // Soft delete: deactivate customer
        // =========================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == id);

            if (customer == null)
                return NotFound(new { message = "Customer not found." });

            customer.IsActive = false;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Customer deactivated successfully."
            });
        }
    }
}