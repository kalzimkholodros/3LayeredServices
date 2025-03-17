using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Data;
using CartService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Redis;

namespace CartService.Services;

public class CartService : ICartService
{
    private readonly CartDbContext _dbContext;
    private readonly IRedisService _redisService;
    private readonly ILogger<CartService> _logger;

    public CartService(CartDbContext dbContext, IRedisService redisService, ILogger<CartService> logger)
    {
        _dbContext = dbContext;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<List<CartItem>> GetCartItemsAsync(string userId)
    {
        var cacheKey = $"cart:{userId}";
        _logger.LogInformation($"Getting cart items from Redis with key: {cacheKey}");
        var cartItems = await _redisService.GetAsync<List<CartItem>>(cacheKey);
        
        if (cartItems == null)
        {
            _logger.LogInformation("Cart items not found in Redis, getting from database");
            cartItems = await _dbContext.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _logger.LogInformation($"Setting cart items to Redis with key: {cacheKey}");
                await _redisService.SetAsync(cacheKey, cartItems);
            }
        }

        return cartItems;
    }

    public async Task<CartItem?> GetCartItemByIdAsync(int id)
    {
        _logger.LogInformation($"Getting cart item by id: {id}");
        return await _dbContext.CartItems.FindAsync(id);
    }

    public async Task<CartItem> AddItemToCartAsync(CartItem item)
    {
        _logger.LogInformation($"Adding item to cart for user: {item.UserId}");
        var existingItem = await _dbContext.CartItems
            .FirstOrDefaultAsync(c => c.UserId == item.UserId && c.ProductId == item.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
            _dbContext.CartItems.Update(existingItem);
        }
        else
        {
            await _dbContext.CartItems.AddAsync(item);
        }

        await _dbContext.SaveChangesAsync();
        await InvalidateCache(item.UserId);
        return item;
    }

    public async Task<CartItem> UpdateCartItemAsync(CartItem item)
    {
        _logger.LogInformation($"Updating cart item for user: {item.UserId}");
        _dbContext.CartItems.Update(item);
        await _dbContext.SaveChangesAsync();
        await InvalidateCache(item.UserId);
        return item;
    }

    public async Task RemoveCartItemAsync(int id)
    {
        var item = await _dbContext.CartItems.FindAsync(id);
        if (item != null)
        {
            _logger.LogInformation($"Removing cart item for user: {item.UserId}");
            _dbContext.CartItems.Remove(item);
            await _dbContext.SaveChangesAsync();
            await InvalidateCache(item.UserId);
        }
    }

    public async Task ClearCartAsync(string userId)
    {
        _logger.LogInformation($"Clearing cart for user: {userId}");
        var items = await _dbContext.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _dbContext.CartItems.RemoveRange(items);
        await _dbContext.SaveChangesAsync();
        await InvalidateCache(userId);
    }

    private async Task InvalidateCache(string userId)
    {
        var cacheKey = $"cart:{userId}";
        _logger.LogInformation($"Invalidating Redis cache with key: {cacheKey}");
        await _redisService.RemoveAsync(cacheKey);
    }
} 