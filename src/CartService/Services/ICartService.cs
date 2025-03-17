using System.Collections.Generic;
using System.Threading.Tasks;
using CartService.Models;

namespace CartService.Services;

public interface ICartService
{
    Task<List<CartItem>> GetCartItemsAsync(string userId);
    Task<CartItem> AddItemToCartAsync(CartItem item);
    Task<CartItem> UpdateCartItemAsync(CartItem item);
    Task<CartItem?> GetCartItemByIdAsync(int id);
    Task RemoveCartItemAsync(int id);
    Task ClearCartAsync(string userId);
} 