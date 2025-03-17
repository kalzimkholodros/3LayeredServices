using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Services;

public class ProductService : IProductService
{
    private readonly ProductDbContext _dbContext;

    public ProductService(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _dbContext.Products.FindAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        await _dbContext.Products.AddAsync(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync();
        return product;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product != null)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        var product = await _dbContext.Products.FindAsync(productId);
        if (product == null || product.StockQuantity < quantity)
        {
            return false;
        }

        product.StockQuantity -= quantity;
        await _dbContext.SaveChangesAsync();
        return true;
    }
} 