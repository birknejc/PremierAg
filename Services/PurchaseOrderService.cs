using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PAS.Services
{
    public class PurchaseOrderService
    {
        private readonly AppDbContext _context;
        private readonly InventoryService _inventoryService;

        public PurchaseOrderService(AppDbContext context, InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        // Fetches a list of all PurchaseOrders, including their items
        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders
                .Include(po => po.Items) // Include items
                .ToListAsync();
        }

        // Fetches a single PurchaseOrder by its ID, including its items
        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Items) // Include items
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        // Adds a new PurchaseOrder and its items to the database
        public async Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            foreach (var item in purchaseOrder.Items)
            {
                var chemical = await _context.Inventories.FindAsync(item.InventoryId);
                if (chemical != null)
                {
                    item.ChemicalName = chemical.ChemicalName;
                    item.UnitOfMeasurePurchase = chemical.UnitOfMeasurePurchase;
                    item.EPANumber = chemical.EPA;
                }
            }

            // Ensure DateTime properties are in UTC
            purchaseOrder.OrderDate = purchaseOrder.OrderDate.ToUniversalTime();
            purchaseOrder.PaymentDueDate = purchaseOrder.PaymentDueDate.ToUniversalTime();
            purchaseOrder.DeliveryPickUpDate = purchaseOrder.DeliveryPickUpDate.ToUniversalTime();

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
        }

        // Updates an existing PurchaseOrder and its items in the database
        public async Task UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            var existingPurchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items) // Include items
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);
            if (existingPurchaseOrder != null)
            {
                // Update the main PurchaseOrder fields
                existingPurchaseOrder.PONumber = purchaseOrder.PONumber;
                existingPurchaseOrder.OrderDate = purchaseOrder.OrderDate.ToUniversalTime();
                existingPurchaseOrder.ReceivedDate = purchaseOrder.ReceivedDate?.ToUniversalTime();
                existingPurchaseOrder.BusinessName = purchaseOrder.BusinessName;
                existingPurchaseOrder.PaymentDueDate = purchaseOrder.PaymentDueDate.ToUniversalTime();
                existingPurchaseOrder.DeliveryPickUpDate = purchaseOrder.DeliveryPickUpDate.ToUniversalTime();
                existingPurchaseOrder.PickUpLocation = purchaseOrder.PickUpLocation;

                // Update the items
                // Remove existing items not in the updated purchase order
                _context.PurchaseOrderItems.RemoveRange(existingPurchaseOrder.Items.Where(ei => !purchaseOrder.Items.Any(ui => ui.Id == ei.Id)));

                // Add or update items
                foreach (var item in purchaseOrder.Items)
                {
                    var existingItem = existingPurchaseOrder.Items.FirstOrDefault(ei => ei.Id == item.Id);
                    if (existingItem != null)
                    {
                        // Update existing item
                        existingItem.InventoryId = item.InventoryId;
                        existingItem.ChemicalName = item.ChemicalName;
                        existingItem.UnitOfMeasurePurchase = item.UnitOfMeasurePurchase;
                        existingItem.Price = item.Price;
                        existingItem.QuantityOrdered = item.QuantityOrdered;
                        existingItem.EPANumber = item.EPANumber;
                    }
                    else
                    {
                        // Add new item
                        existingPurchaseOrder.Items.Add(item);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        // Deletes a PurchaseOrder and its items from the database
        public async Task DeletePurchaseOrderAsync(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.Items) // Include items
                .FirstOrDefaultAsync(po => po.Id == id);
            if (purchaseOrder != null)
            {
                _context.PurchaseOrders.Remove(purchaseOrder);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReceivePurchaseOrderItemAsync(int purchaseOrderItemId, int quantityReceived)
        {
            var purchaseOrderItem = await _context.PurchaseOrderItems.FirstOrDefaultAsync(poi => poi.Id == purchaseOrderItemId);
            if (purchaseOrderItem == null) throw new Exception("Purchase Order Item not found.");

            var inventory = await _inventoryService.GetInventoryByIdAsync(purchaseOrderItem.InventoryId);
            if (inventory == null) throw new Exception("Inventory item not found.");

            // Update the quantity received and inventory
            purchaseOrderItem.QuantityReceived += quantityReceived; // Add only the newly received quantity
            inventory.QuantityOnHand += quantityReceived;

            if (purchaseOrderItem.Price > inventory.WeightedAveragePrice)
            {
                decimal existingQuantity = inventory.QuantityOnHand - quantityReceived;
                decimal totalQuantity = existingQuantity + quantityReceived;
                decimal totalCost = (existingQuantity * inventory.WeightedAveragePrice) + (quantityReceived * purchaseOrderItem.Price);
                inventory.WeightedAveragePrice = Math.Round(totalCost / totalQuantity, 3);
            }

            await _context.SaveChangesAsync();

            // Check if the entire purchase order is fully received
            var purchaseOrder = await GetPurchaseOrderByIdAsync(purchaseOrderItem.PurchaseOrderId);
            if (purchaseOrder.Items.All(item => item.QuantityReceived >= item.QuantityOrdered))
            {
                purchaseOrder.ReceivedDate = DateTime.UtcNow;
                await UpdatePurchaseOrderAsync(purchaseOrder);
            }
        }
        public async Task ReceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> itemQuantities)
        {
            foreach (var item in itemQuantities)
            {
                await ReceivePurchaseOrderItemAsync(item.Key, item.Value);
            }
        }
        public async Task UnreceivePurchaseOrderItemAsync(int purchaseOrderItemId, int quantityUnreceived)
        {
            var purchaseOrderItem = await _context.PurchaseOrderItems.FirstOrDefaultAsync(poi => poi.Id == purchaseOrderItemId);
            if (purchaseOrderItem == null) throw new Exception("Purchase Order Item not found.");

            var inventory = await _inventoryService.GetInventoryByIdAsync(purchaseOrderItem.InventoryId);
            if (inventory == null) throw new Exception("Inventory item not found.");

            purchaseOrderItem.QuantityReceived -= quantityUnreceived;
            inventory.QuantityOnHand -= quantityUnreceived;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Inventory>> GetChemicalsAsync()
        {
            return await _context.Inventories.ToListAsync(); // Assuming Inventory is the name of your inventory DbSet
        }

        public async Task UnreceivePurchaseOrderAsync(int purchaseOrderId, Dictionary<int, int> itemQuantities)
        {
            foreach (var item in itemQuantities)
            {
                await UnreceivePurchaseOrderItemAsync(item.Key, item.Value);
            }
        }

    }
}
