using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;
using Shared.RabbitMQ;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IRabbitMQService _rabbitMQService;

    public OrderController(IOrderService orderService, IRabbitMQService rabbitMQService)
    {
        _orderService = orderService;
        _rabbitMQService = rabbitMQService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<Order>>> GetUserOrders(string userId)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        var result = await _orderService.CreateOrderAsync(order);
        await _rabbitMQService.PublishMessage("order-created", result);
        return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<Order>> UpdateOrderStatus(int id, [FromBody] string status)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, status);
        if (order == null)
        {
            return NotFound();
        }

        await _rabbitMQService.PublishMessage("order-status-updated", new { OrderId = id, Status = status });
        return Ok(order);
    }
} 