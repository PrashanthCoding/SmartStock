using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Application.DTOs.Product;
using SmartStock.Application.Interfaces.Services;

namespace SmartStock.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // ======================================
    // GET: api/products
    // ======================================
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductSearchRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductsAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    // ======================================
    // GET: api/products/1
    // ======================================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductByIdAsync(
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new
            {
                Success = false,
                Message = $"Product with ID {id} was not found."
            });
        }

        return Ok(result);
    }

    // ======================================
    // POST: api/products
    // ======================================
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateProductAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetProductById),
            new { id = result.Id },
            result);
    }

    // ======================================
    // PUT: api/products/1
    // ======================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id,
        [FromBody] UpdateProductDto request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateProductAsync(
            id,
            request,
            cancellationToken);

        if (result is null)
        {
            return NotFound(new
            {
                Success = false,
                Message = $"Active product with ID {id} was not found."
            });
        }

        return Ok(result);
    }

    // ======================================
    // DELETE: api/products/1
    // ======================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateProduct(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.DeactivateProductAsync(
            id,
            cancellationToken);

        if (!result)
        {
            return NotFound(new
            {
                Success = false,
                Message = $"Active product with ID {id} was not found."
            });
        }

        return NoContent();
    }
}
