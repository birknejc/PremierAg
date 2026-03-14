using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static PAS.Models.PurchaseOrderItem;

namespace PAS.Services
{
    public class PurchaseOrderService
    {
        private readonly AppDbContext _context;
        //private readonly InventoryService _inventoryService;
        private readonly IWebHostEnvironment _env;
        private readonly ProductInventoryService _productInventoryService;

        public PurchaseOrderService(
            AppDbContext context,
            //InventoryService inventoryService,
            ProductInventoryService productInventoryService,
            IWebHostEnvironment env)
        {
            _context = context;
            //_inventoryService = inventoryService;
            _productInventoryService = productInventoryService;
            _env = env;
        }

        // ---------------------------------------------------------
        // GET PURCHASE ORDERS
        // ---------------------------------------------------------
        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Items)
                .ToListAsync();
        }

        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        // ---------------------------------------------------------
        // ADD PURCHASE ORDER
        // ---------------------------------------------------------

        public async Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            foreach (var item in purchaseOrder.Items)
            {
                if (item.ProductId != null)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product != null)
                    {
                        item.ProductName = product.Name;
                        item.ProductEPA = product.EPA;
                        item.ProductUOM = product.DefaultUnitOfMeasure;
                        item.ProductPurchasePrice = item.Price;

                        item.ChemicalName = product.Name;
                        item.UnitOfMeasurePurchase = product.DefaultUnitOfMeasure;
                        item.EPANumber = product.EPA;
                    }
                }

                // NEW: initialize workflow fields
                item.Status = PurchaseOrderStatus.Open;
                item.RemainingQuantity = item.QuantityOrdered;
            }

            purchaseOrder.OrderDate = purchaseOrder.OrderDate.ToUniversalTime();
            purchaseOrder.PaymentDueDate = purchaseOrder.PaymentDueDate?.ToUniversalTime();
            purchaseOrder.DeliveryPickUpDate = purchaseOrder.DeliveryPickUpDate?.ToUniversalTime();

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
        }



        // ---------------------------------------------------------
        // UPDATE PURCHASE ORDER
        // ---------------------------------------------------------

        public async Task UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            var existingPurchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);

            if (existingPurchaseOrder == null)
                return;

            // -----------------------------
            // Update PO header
            // -----------------------------
            existingPurchaseOrder.PONumber = purchaseOrder.PONumber;
            existingPurchaseOrder.OrderDate = purchaseOrder.OrderDate.ToUniversalTime();
            existingPurchaseOrder.ReceivedDate = purchaseOrder.ReceivedDate?.ToUniversalTime();
            existingPurchaseOrder.VendorId = purchaseOrder.VendorId;
            existingPurchaseOrder.BusinessName = purchaseOrder.BusinessName;
            existingPurchaseOrder.PaymentDueDate = purchaseOrder.PaymentDueDate?.ToUniversalTime();
            existingPurchaseOrder.DeliveryPickUpDate = purchaseOrder.DeliveryPickUpDate?.ToUniversalTime();
            existingPurchaseOrder.PickUpLocation = purchaseOrder.PickUpLocation;

            // -----------------------------
            // Remove deleted items
            // -----------------------------
            _context.PurchaseOrderItems.RemoveRange(
                existingPurchaseOrder.Items.Where(ei => !purchaseOrder.Items.Any(ui => ui.Id == ei.Id))
            );

            // -----------------------------
            // Add or update items
            // -----------------------------
            foreach (var item in purchaseOrder.Items)
            {
                var existingItem = existingPurchaseOrder.Items.FirstOrDefault(ei => ei.Id == item.Id);

                if (existingItem != null)
                {
                    // Update editable fields
                    existingItem.Price = item.Price;
                    existingItem.QuantityOrdered = item.QuantityOrdered;

                    // Recalculate RemainingQuantity ONLY if PO is still open
                    if (existingItem.Status == PurchaseOrderStatus.Open)
                    {
                        existingItem.RemainingQuantity =
                            existingItem.QuantityOrdered - existingItem.QuantityReceived;
                    }

                    // Update product snapshot fields
                    if (item.ProductId != null)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);

                        existingItem.ProductId = item.ProductId;
                        existingItem.ProductName = product.Name;
                        existingItem.ProductEPA = product.EPA;
                        existingItem.ProductUOM = product.DefaultUnitOfMeasure;
                        existingItem.ProductPurchasePrice = item.Price;

                        // Legacy compatibility
                        existingItem.ChemicalName = product.Name;
                        existingItem.UnitOfMeasurePurchase = product.DefaultUnitOfMeasure;
                        existingItem.EPANumber = product.EPA;
                    }
                }
                else
                {
                    // -----------------------------
                    // NEW ITEM ADDED TO EXISTING PO
                    // -----------------------------
                    if (item.ProductId != null)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);

                        item.ProductName = product.Name;
                        item.ProductEPA = product.EPA;
                        item.ProductUOM = product.DefaultUnitOfMeasure;
                        item.ProductPurchasePrice = item.Price;

                        // Legacy compatibility
                        item.ChemicalName = product.Name;
                        item.UnitOfMeasurePurchase = product.DefaultUnitOfMeasure;
                        item.EPANumber = product.EPA;
                    }

                    // Initialize workflow fields
                    item.Status = PurchaseOrderStatus.Open;
                    item.RemainingQuantity = item.QuantityOrdered;

                    existingPurchaseOrder.Items.Add(item);
                }
            }

            await _context.SaveChangesAsync();
        }



        // ---------------------------------------------------------
        // DELETE PURCHASE ORDER
        // ---------------------------------------------------------
        public async Task DeletePurchaseOrderAsync(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder != null)
            {
                _context.PurchaseOrders.Remove(purchaseOrder);
                await _context.SaveChangesAsync();
            }
        }

        // ---------------------------------------------------------
        // RECEIVE PURCHASE ORDER ITEM
        // ---------------------------------------------------------
        public async Task ReceivePurchaseOrderItemAsync(int purchaseOrderItemId, decimal quantityReceived)
        {
            var purchaseOrderItem = await _context.PurchaseOrderItems
                .FirstOrDefaultAsync(poi => poi.Id == purchaseOrderItemId);

            if (purchaseOrderItem == null)
                throw new Exception("Purchase Order Item not found.");

            if (purchaseOrderItem.Status == PurchaseOrderStatus.Closed)
                throw new Exception("Cannot receive a closed Purchase Order item.");

            var purchaseOrder = await _context.PurchaseOrders
                .FirstOrDefaultAsync(po => po.Id == purchaseOrderItem.PurchaseOrderId);

            if (purchaseOrder == null)
                throw new Exception("Purchase Order not found.");

            bool isProduct = purchaseOrderItem.ProductId != null;

            // Create inventory batch if this is a product
            if (isProduct)
            {
                var purchase = new ProductPurchase
                {
                    ProductId = purchaseOrderItem.ProductId.Value,
                    QuantityReceived = quantityReceived,
                    PricePerUnit = purchaseOrderItem.ProductPurchasePrice,
                    VendorId = purchaseOrder.VendorId
                };

                await _productInventoryService.ReceivePurchaseAsync(purchase);
            }

            // -----------------------------
            // Update workflow fields
            // -----------------------------
            purchaseOrderItem.QuantityReceived += quantityReceived;

            purchaseOrderItem.RemainingQuantity =
                purchaseOrderItem.QuantityOrdered - purchaseOrderItem.QuantityReceived;

            // Auto-close item if exact received
            if (purchaseOrderItem.RemainingQuantity <= 0)
            {
                purchaseOrderItem.Status = PurchaseOrderStatus.Closed;
            }

            await _context.SaveChangesAsync();

            // -----------------------------
            // Auto-close PO if all items are closed
            // -----------------------------
            purchaseOrder = await GetPurchaseOrderByIdAsync(purchaseOrderItem.PurchaseOrderId);

            if (purchaseOrder.Items.All(i => i.Status == PurchaseOrderStatus.Closed))
            {
                purchaseOrder.ReceivedDate = DateTime.UtcNow;
                await UpdatePurchaseOrderAsync(purchaseOrder);
            }
        }

        public async Task ReceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, decimal> itemQuantities)
        {
            foreach (var item in itemQuantities)
                await ReceivePurchaseOrderItemAsync(item.Key, item.Value);
        }

        // ---------------------------------------------------------
        // UNRECEIVE (LEGACY ONLY)
        // ---------------------------------------------------------
        public async Task UnreceivePurchaseOrderItemAsync(int purchaseOrderItemId, int quantityUnreceived)
        {
            var purchaseOrderItem = await _context.PurchaseOrderItems
                .FirstOrDefaultAsync(poi => poi.Id == purchaseOrderItemId);

            if (purchaseOrderItem == null)
                throw new Exception("Purchase Order Item not found.");

            purchaseOrderItem.QuantityReceived -= quantityUnreceived;
            //inventory.QuantityOnHand -= quantityUnreceived;

            await _context.SaveChangesAsync();
        }

        public async Task UnreceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> itemQuantities)
        {
            foreach (var item in itemQuantities)
                await UnreceivePurchaseOrderItemAsync(item.Key, item.Value);
        }

        // ---------------------------------------------------------
        // PRINTING
        // ---------------------------------------------------------
        public async Task<string> BuildPurchaseOrderPrintHtmlAsync(int purchaseOrderId)
        {
            var purchaseOrder = await GetPurchaseOrderByIdAsync(purchaseOrderId);
            if (purchaseOrder == null)
                return "<h3>Purchase Order not found.</h3>";

            var templatePath = Path.Combine(_env.WebRootPath, "print_potemplate.html");
            var template = await File.ReadAllTextAsync(templatePath);

            var itemsHtml = purchaseOrder.Items != null
                ? string.Join("", purchaseOrder.Items.Select(item =>
                    $@"<tr>
                        <td>{item.ChemicalName}</td>
                        <td>{item.UnitOfMeasurePurchase}</td>
                        <td>{item.Price}</td>
                        <td>{item.QuantityOrdered}</td>
                        <td>{item.QuantityReceived}</td>
                        <td>{item.TotalCost.ToString("0.00")}</td>
                    </tr>"
                ))
                : "";

            template = template
                .Replace("{{PONumber}}", purchaseOrder.PONumber)
                .Replace("{{OrderDate}}", purchaseOrder.OrderDate.ToString("MM/dd/yyyy"))
                .Replace("{{BusinessName}}", purchaseOrder.BusinessName)
                .Replace("{{PaymentDueDate}}", purchaseOrder.PaymentDueDate?.ToString("MM/dd/yyyy") ?? "")
                .Replace("{{DeliveryPickUpDate}}", purchaseOrder.DeliveryPickUpDate?.ToString("MM/dd/yyyy") ?? "")
                .Replace("{{PickUpLocation}}", purchaseOrder.PickUpLocation)
                .Replace("{{ItemsRows}}", itemsHtml);

            return template;
        }

        public async Task ReturnPurchaseOrderItemsAsync(int purchaseOrderId, Dictionary<int, decimal> quantitiesToReturn)
        {
            foreach (var kvp in quantitiesToReturn)
            {
                int itemId = kvp.Key;
                decimal qtyReturned = kvp.Value;

                await ReturnPurchaseOrderItemAsync(itemId, qtyReturned);
            }
        }


        public async Task ReturnPurchaseOrderItemAsync(int purchaseOrderItemId, decimal quantityReturned)
        {
            if (quantityReturned <= 0)
                throw new Exception("Return quantity must be greater than zero.");

            var purchaseOrderItem = await _context.PurchaseOrderItems
                .FirstOrDefaultAsync(poi => poi.Id == purchaseOrderItemId);

            if (purchaseOrderItem == null)
                throw new Exception("Purchase Order Item not found.");

            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrderItem.PurchaseOrderId);

            if (purchaseOrder == null)
                throw new Exception("Purchase Order not found.");

            if (quantityReturned > purchaseOrderItem.QuantityReceived)
                throw new Exception("Cannot return more than has been received.");

            bool isProduct = purchaseOrderItem.ProductId != null;

            // -----------------------------
            // Reverse inventory (negative batch)
            // -----------------------------
            if (isProduct)
            {
                var purchase = new ProductPurchase
                {
                    ProductId = purchaseOrderItem.ProductId.Value,
                    QuantityReceived = -quantityReturned,   // negative = return
                    PricePerUnit = purchaseOrderItem.ProductPurchasePrice,
                    VendorId = purchaseOrder.VendorId
                };

                await _productInventoryService.ReceivePurchaseAsync(purchase);
            }

            // -----------------------------
            // Update workflow fields
            // -----------------------------
            purchaseOrderItem.QuantityReceived -= quantityReturned;

            purchaseOrderItem.RemainingQuantity =
                purchaseOrderItem.QuantityOrdered - purchaseOrderItem.QuantityReceived;

            // Reopen item if it was previously closed
            if (purchaseOrderItem.Status == PurchaseOrderStatus.Closed &&
                purchaseOrderItem.RemainingQuantity > 0)
            {
                purchaseOrderItem.Status = PurchaseOrderStatus.Open;
            }

            await _context.SaveChangesAsync();

            // -----------------------------
            // Reopen PO if any item is open
            // -----------------------------
            if (purchaseOrder.Items.Any(i => i.Status == PurchaseOrderStatus.Open))
            {
                purchaseOrder.ReceivedDate = null;
                await UpdatePurchaseOrderAsync(purchaseOrder);
            }
        }


        public async Task ClosePurchaseOrderAsync(int purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);

            if (purchaseOrder == null)
                throw new Exception("Purchase Order not found.");

            foreach (var item in purchaseOrder.Items)
            {
                item.Status = PurchaseOrderStatus.Closed;
                item.RemainingQuantity = 0;
            }

            purchaseOrder.ReceivedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


    }
}
