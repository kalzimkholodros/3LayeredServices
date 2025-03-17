using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Models;

namespace OrderService.Services;

public interface IOrderService
{
    Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    Task<Order> GetOrderByIdAsync(int id);
    Task<Order> CreateOrderAsync(Order order);
    Task<Order> UpdateOrderStatusAsync(int orderId, string status);
    Task<List<Order>> GetAllOrdersAsync();
} 