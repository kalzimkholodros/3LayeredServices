using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CartService.Models;
using CartService.Services;
using Shared.RabbitMQ;

namespace CartService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IRabbitMQService _rabbitMQService;

    public CartController(ICartService cartService, IRabbitMQService rabbitMQService)
    {
        _cartService = cartService;
        _rabbitMQService = rabbitMQService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CartItem>>> GetAllCartItems()
    {
        var cartItems = await _cartService.GetCartItemsAsync("1");
        return Ok(cartItems);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<List<CartItem>>> GetCartItems(string userId)
    {
        var cartItems = await _cartService.GetCartItemsAsync(userId);
        return Ok(cartItems);
    }

    [HttpPost]
    public async Task<ActionResult<CartItem>> AddItemToCart([FromBody] CartItem cartItem)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var addedItem = await _cartService.AddItemToCartAsync(cartItem);
        await _rabbitMQService.PublishMessage("cart-updated", addedItem);
        return CreatedAtAction(nameof(GetCartItems), new { userId = addedItem.UserId }, addedItem);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CartItem>> UpdateCartItem(int id, [FromBody] CartItem cartItem)
    {
        if (id != cartItem.Id)
        {
            return BadRequest();
        }

        var existingItem = await _cartService.GetCartItemByIdAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }

        var updatedItem = await _cartService.UpdateCartItemAsync(cartItem);
        await _rabbitMQService.PublishMessage("cart-updated", updatedItem);
        return Ok(updatedItem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveCartItem(int id)
    {
        var item = await _cartService.GetCartItemByIdAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        await _cartService.RemoveCartItemAsync(id);
        return NoContent();
    }

    [HttpDelete("clear/{userId}")]
    public async Task<IActionResult> ClearCart(string userId)
    {
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
} 