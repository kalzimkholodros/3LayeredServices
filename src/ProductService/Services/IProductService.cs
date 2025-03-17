using System.Collections.Generic;
using System.Threading.Tasks;
using ProductService.Models;

namespace ProductService.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
    Task<bool> UpdateStockAsync(int productId, int quantity);
} 