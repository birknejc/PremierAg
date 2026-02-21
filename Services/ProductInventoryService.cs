using Microsoft.EntityFrameworkCore;
using PAS.DBContext;
using PAS.Models;

namespace PAS.Services
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly AppDbContext _context;

        public ProductInventoryService(AppDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------
        //  PRODUCT QUERIES
        // ------------------------------------------------------------

        public async Task<Product> GetProductAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductVendors)
                    .ThenInclude(pv => pv.Vendor)
                .Include(p => p.Purchases)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<decimal> GetAvailableQuantityAsync(int productId)
        {
            var product = await GetProductAsync(productId);
            return product?.TotalQuantityAvailable ?? 0;
        }

        public async Task<decimal> GetWeightedAveragePriceAsync(int productId)
        {
            var product = await GetProductAsync(productId);
            return product?.WeightedAveragePrice ?? 0;
        }

        // ------------------------------------------------------------
        //  RECEIVE PURCHASE (UPDATES WEIGHTED AVERAGE)
        // ------------------------------------------------------------

        public async Task ReceivePurchaseAsync(ProductPurchase purchase)
        {
            var product = await _context.Products
                .Include(p => p.Purchases)
                .FirstOrDefaultAsync(p => p.Id == purchase.ProductId);

            if (product == null)
                throw new Exception("Product not found.");

            // Old totals
            decimal oldQty = product.Purchases.Sum(pp => pp.QuantityRemaining);
            decimal oldAvg = product.WeightedAveragePrice;

            // New purchase
            decimal newQty = purchase.QuantityReceived;
            decimal newPrice = purchase.PricePerUnit;

            // Weighted average formula
            decimal newWeightedAvg =
                (oldQty * oldAvg + newQty * newPrice) /
                (oldQty + newQty);

            product.WeightedAveragePrice = newWeightedAvg;

            // Add purchase record
            purchase.QuantityRemaining = purchase.QuantityReceived;
            purchase.ReceivedDate = DateTime.UtcNow;

            _context.ProductPurchases.Add(purchase);

            await _context.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        //  CONSUME INVENTORY (FIFO)
        // ------------------------------------------------------------

        public async Task ConsumeInventoryAsync(int productId, decimal quantityUsed)
        {
            var product = await _context.Products
                .Include(p => p.Purchases.OrderBy(pp => pp.ReceivedDate))
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found.");

            decimal remainingToConsume = quantityUsed;

            foreach (var purchase in product.Purchases.OrderBy(pp => pp.ReceivedDate))
            {
                if (remainingToConsume <= 0)
                    break;

                if (purchase.QuantityRemaining >= remainingToConsume)
                {
                    purchase.QuantityRemaining -= remainingToConsume;
                    remainingToConsume = 0;
                }
                else
                {
                    remainingToConsume -= purchase.QuantityRemaining;
                    purchase.QuantityRemaining = 0;
                }
            }

            if (remainingToConsume > 0)
                throw new Exception("Not enough inventory available.");

            await _context.SaveChangesAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Purchases)
                .ToListAsync();
        }

    }
}

