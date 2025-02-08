using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class PurchaseOrderService
    {
        private readonly AppDbContext _context;

        public PurchaseOrderService(AppDbContext context)
        {
            _context = context;
        }

        // Fetches a list of all PurchaseOrders from the database
        public async Task<List<PurchaseOrder>> GetPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders.ToListAsync();
        }

        // Fetches a single PurchaseOrder by its ID
        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(int id)
        {
            return await _context.PurchaseOrders.FindAsync(id);

        }

        // Adds a new PurchaseOrder to the database
        public async Task AddPurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            var chemical = await _context.Inventories.FindAsync(purchaseOrder.InventoryId);
            if (chemical != null)
            {
                purchaseOrder.ChemicalName = chemical.ChemicalName;  // Set the ChemicalName from the Inventory table
            }

            // Ensure DateTime properties are in UTC
            purchaseOrder.OrderDate = purchaseOrder.OrderDate.ToUniversalTime();
            purchaseOrder.PaymentDueDate = purchaseOrder.PaymentDueDate.ToUniversalTime();
            purchaseOrder.DeliveryPickUpDate = purchaseOrder.DeliveryPickUpDate.ToUniversalTime();

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();
        }


        // Updates an existing PurchaseOrder in the database
        public async Task UpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            var existingPurchaseOrder = await _context.PurchaseOrders.Include(po => po.Inventory)
                                                                       .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);
            if (existingPurchaseOrder != null)
            {
                existingPurchaseOrder.PONumber = purchaseOrder.PONumber;
                existingPurchaseOrder.OrderDate = purchaseOrder.OrderDate;
                existingPurchaseOrder.ReceivedDate = purchaseOrder.ReceivedDate;
                existingPurchaseOrder.BusinessName = purchaseOrder.BusinessName;
                existingPurchaseOrder.EPANumber = purchaseOrder.EPANumber;
                existingPurchaseOrder.UnitOfMeasure = purchaseOrder.UnitOfMeasure;
                existingPurchaseOrder.Price = purchaseOrder.Price;
                existingPurchaseOrder.QuantityOrdered = purchaseOrder.QuantityOrdered;
                existingPurchaseOrder.PaymentDueDate = purchaseOrder.PaymentDueDate;
                existingPurchaseOrder.DeliveryPickUpDate = purchaseOrder.DeliveryPickUpDate;
                existingPurchaseOrder.PickUpLocation = purchaseOrder.PickUpLocation;

                // Set the ChemicalName from Inventory
                var chemical = await _context.Inventories.FindAsync(purchaseOrder.InventoryId);
                if (chemical != null)
                {
                    existingPurchaseOrder.ChemicalName = chemical.ChemicalName;
                }

                _context.PurchaseOrders.Update(existingPurchaseOrder);
                await _context.SaveChangesAsync();
            }
        }


        // Deletes a PurchaseOrder from the database by its ID
        public async Task DeletePurchaseOrderAsync(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder != null)
            {
                _context.PurchaseOrders.Remove(purchaseOrder);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Inventory>> GetChemicalsAsync()
        {
            return await _context.Inventories.ToListAsync(); // Assuming Inventory is the name of your inventory DbSet
        }
    }
}