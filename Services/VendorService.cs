using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class VendorService
    {
        private readonly AppDbContext _context;

        public VendorService(AppDbContext context)
        {
            _context = context;
        }

        // Get all vendors
        public async Task<List<Vendor>> GetVendorsAsync()
        {
            return await _context.Vendors.ToListAsync();
        }

        // Get a single vendor by ID
        public async Task<Vendor?> GetVendorByIdAsync(int id)
        {
            return await _context.Vendors.FindAsync(id);
        }

        // Add a new vendor
        public async Task AddVendorAsync(Vendor vendor)
        {
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();
        }

        // Update an existing vendor
        public async Task UpdateVendorAsync(Vendor vendor)
        {
            var existingVendor = await _context.Vendors.FindAsync(vendor.Id);
            if (existingVendor != null)
            {
                existingVendor.BusinessName = vendor.BusinessName;
                existingVendor.StreetAddress = vendor.StreetAddress;
                existingVendor.City = vendor.City;
                existingVendor.ZipCode = vendor.ZipCode;
                existingVendor.Phone = vendor.Phone;
                existingVendor.Fax = vendor.Fax;
                existingVendor.Email = vendor.Email;
                existingVendor.SalesRepName = vendor.SalesRepName;
                existingVendor.SalesRepPhone = vendor.SalesRepPhone;
                existingVendor.SalesRepEmail = vendor.SalesRepEmail;

                _context.Vendors.Update(existingVendor);
                await _context.SaveChangesAsync();
            }
        }

        // Delete a vendor by ID
        public async Task DeleteVendorAsync(int id)
        {
            var vendor = await _context.Vendors.FindAsync(id);
            if (vendor != null)
            {
                _context.Vendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }
        }
    }

}
