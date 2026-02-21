using PAS.Models;

namespace PAS.Services
{
    public interface IProductInventoryService
    {
        Task<Product> GetProductAsync(int productId);
        Task<decimal> GetAvailableQuantityAsync(int productId);
        Task<decimal> GetWeightedAveragePriceAsync(int productId);

        Task ReceivePurchaseAsync(ProductPurchase purchase);
        Task ConsumeInventoryAsync(int productId, decimal quantityUsed);

        Task<List<Product>> GetAllProductsAsync();

    }
}

