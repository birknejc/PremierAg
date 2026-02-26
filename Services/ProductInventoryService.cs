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

            product.WeightedAveragePrice = Math.Round(newWeightedAvg, 2);

            // Add purchase record
            purchase.QuantityRemaining = purchase.QuantityReceived;
            purchase.ReceivedDate = DateTime.UtcNow;

            _context.ProductPurchases.Add(purchase);

            await _context.SaveChangesAsync();
        }

        // ------------------------------------------------------------
        //  CONSUME INVENTORY (FIFO)
        // ------------------------------------------------------------

        public async Task ConsumeInventoryAsync(
            int productId,
            decimal quantityUsed,
            int groupId,
            int loadMixId,
            int loadMixDetailsId)
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

                // How much we will deduct from THIS purchase
                decimal amountFromThisPurchase = 0;

                if (purchase.QuantityRemaining >= remainingToConsume)
                {
                    amountFromThisPurchase = remainingToConsume;
                    purchase.QuantityRemaining -= remainingToConsume;
                    remainingToConsume = 0;
                }
                else
                {
                    amountFromThisPurchase = purchase.QuantityRemaining;
                    remainingToConsume -= purchase.QuantityRemaining;
                    purchase.QuantityRemaining = 0;
                }

                // ⭐ AUDIT LOG ENTRY (negative = deduction)
                await AddAuditRecord(
                    productId,
                    product.Name,
                    -amountFromThisPurchase,
                    groupId,
                    loadMixId,
                    loadMixDetailsId,
                    "Consume");
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

        public async Task RestoreInventoryAsync(
            int productId,
            decimal quantityToRestore,
            int groupId,
            int loadMixId,
            int loadMixDetailsId)
        {
            if (quantityToRestore <= 0)
                return;

            var product = await _context.Products
                .Include(p => p.Purchases)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                throw new Exception("Product not found.");

            // ⭐ Restore into the most recent purchase (LIFO)
            var latestPurchase = product.Purchases
                .OrderByDescending(pp => pp.ReceivedDate)
                .FirstOrDefault();

            if (latestPurchase == null)
                throw new Exception("No purchases found to restore inventory into.");

            latestPurchase.QuantityRemaining += quantityToRestore;

            // ⭐ AUDIT LOG ENTRY (positive = restoration)
            await AddAuditRecord(
                productId,
                product.Name,
                quantityToRestore,     // positive = restore
                groupId,
                loadMixId,
                loadMixDetailsId,
                "Restore");

            await _context.SaveChangesAsync();
        }


        private async Task AddAuditRecord(
            int productId,
            string productName,
            decimal quantityChanged,
            int groupId,
            int loadMixId,
            int loadMixDetailsId,
            string action)
        {
            var audit = new InventoryAudit
            {
                ProductId = productId,
                ProductName = productName,
                QuantityChanged = quantityChanged, // negative = consume, positive = restore
                GroupId = groupId,
                LoadMixId = loadMixId,
                LoadMixDetailsId = loadMixDetailsId,
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            _context.InventoryAudits.Add(audit);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetHoldQuantityAsync(int productId)
        {
            var today = DateTime.UtcNow.Date;

            // Load all active, unexpired quotes for this product
            var quoteItems = await _context.Quotes
                .Where(q => q.Status == QuoteStatus.Active && q.ArchiveDate > today)
                .SelectMany(q => q.QuoteInventories
                    .Where(qi => qi.ProductId == productId)
                    .Select(qi => new
                    {
                        qi.QuantityPerAcre,
                        qi.QuoteUnitOfMeasure,
                        q.EstimatedAcres
                    }))
                .ToListAsync();

            decimal totalHold = 0;

            foreach (var item in quoteItems)
            {
                // QUOM (e.g., fl oz)
                var quom = item.QuoteUnitOfMeasure?.ToLower().Trim();

                // PUOM (product’s sold unit)
                var product = await _context.Products.FindAsync(productId);
                var puom = product.DefaultUnitOfMeasure?.ToLower().Trim();

                // Find conversion
                var conversion = await _context.UOMConversions
                    .FirstOrDefaultAsync(c =>
                        c.QUOM.ToLower().Trim() == quom &&
                        c.PUOM.ToLower().Trim() == puom);

                decimal factor = conversion?.ConversionFactor ?? 1.0M;

                // Convert QUOM → PUOM
                decimal qtyInSoldUnits = (item.QuantityPerAcre * item.EstimatedAcres) / factor;

                totalHold += qtyInSoldUnits;
            }

            return Math.Round(totalHold, 2);
        }

        public async Task AddStartingInventoryAsync(int productId, decimal quantity, decimal cost, string note)
        {
            const int systemVendorId = 1; // ID of "System Initialization" vendor

            var purchase = new ProductPurchase
            {
                ProductId = productId,
                VendorId = systemVendorId,
                QuantityReceived = quantity,
                QuantityRemaining = quantity,
                PricePerUnit = cost,
                ReceivedDate = DateTime.UtcNow,
                PurchaseOrderId = null
            };

            _context.ProductPurchases.Add(purchase);
            await _context.SaveChangesAsync();
        }

    }
}

