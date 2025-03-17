using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Services;
using Shared.RabbitMQ;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IRabbitMQService _rabbitMQService;

    public ProductController(IProductService productService, IRabbitMQService rabbitMQService)
    {
        _productService = productService;
        _rabbitMQService = rabbitMQService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        var result = await _productService.CreateProductAsync(product);
        await _rabbitMQService.PublishMessage("product-created", result);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        var result = await _productService.UpdateProductAsync(product);
        await _rabbitMQService.PublishMessage("product-updated", result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        await _rabbitMQService.PublishMessage("product-deleted", id);
        return NoContent();
    }

    [HttpPut("{id}/stock")]
    public async Task<ActionResult> UpdateStock(int id, [FromBody] int quantity)
    {
        var success = await _productService.UpdateStockAsync(id, quantity);
        if (!success)
        {
            return BadRequest("Insufficient stock");
        }

        await _rabbitMQService.PublishMessage("stock-updated", new { ProductId = id, Quantity = quantity });
        return Ok();
    }
} 