using PAS.DBContext;
using PAS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _context;

        public InventoryService(AppDbContext context)
        {
            _context = context;
        }

        // Get all inventory items along with the vendor details
        public async Task<List<Inventory>> GetAllInventoriesAsync()
        {
            return await _context.Inventories
                .Include(i => i.Vendor)  // Assuming the Inventory has a Vendor navigation property
                .ToListAsync();
        }

        // Get a specific inventory item by its ID
        public async Task<Inventory> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories
                .Include(i => i.Vendor)  // Including vendor details for the inventory
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        // Add a new inventory item
        public async Task AddInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
        }

        // Update an existing inventory item
        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        // Delete an inventory item by its ID
        public async Task DeleteInventoryAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }

        // Get all vendors for the vendor dropdown
        public async Task<List<Vendor>> GetAllVendorsAsync()
        {
            return await _context.Vendors.ToListAsync();
        }
    }
}