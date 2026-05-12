namespace ClinicManagementSystem.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.IsActive = true;
        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        var existingProduct = await _productRepository.GetByIdAsync(product.Id);
        if (existingProduct is null)
            throw new InvalidOperationException("Product not found");

        existingProduct.Name = product.Name;
        existingProduct.QuantityInStock = product.QuantityInStock;
        existingProduct.Unit = product.Unit;
        existingProduct.CostPrice = product.CostPrice;
        existingProduct.MinimumQuantity = product.MinimumQuantity;
        existingProduct.IsActive = product.IsActive;

        _productRepository.Update(existingProduct);
        await _productRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new InvalidOperationException("Product not found");

        product.IsActive = false;
        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync()
    {
        return await _productRepository.GetWithLowStockAsync();
    }

    public async Task<int> GetLowStockCountAsync()
    {
        var lowStockProducts = await _productRepository.GetWithLowStockAsync();
        return lowStockProducts.Count;
    }
}
