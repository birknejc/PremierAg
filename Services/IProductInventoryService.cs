using PAS.Models;

namespace PAS.Services
{
    public interface IProductInventoryService
    {
        Task<Product> GetProductAsync(int productId);
        Task<decimal> GetAvailableQuantityAsync(int productId);
        Task<decimal> GetWeightedAveragePriceAsync(int productId);

        Task ReceivePurchaseAsync(ProductPurchase purchase);

        // ⭐ UPDATED SIGNATURES (must match ProductInventoryService)
        Task ConsumeInventoryAsync(
            int productId,
            decimal quantityUsed,
            int groupId,
            int loadMixId,
            int loadMixDetailsId);

        Task RestoreInventoryAsync(
            int productId,
            decimal quantityToRestore,
            int groupId,
            int loadMixId,
            int loadMixDetailsId);

        Task<List<Product>> GetAllProductsAsync();
    }
}
