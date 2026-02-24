using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Text;

namespace PAS.Services
{
    public class ApplicatorLicenseService
    {
        private readonly AppDbContext _context;

        public ApplicatorLicenseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicatorLicense>> GetAllAsync()
        {
            return await _context.ApplicatorLicenses
                .OrderBy(a => a.LicenseType)
                .ThenBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<ApplicatorLicense?> GetByIdAsync(int id)
        {
            return await _context.ApplicatorLicenses.FindAsync(id);
        }

        public async Task AddAsync(ApplicatorLicense license)
        {
            _context.ApplicatorLicenses.Add(license);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ApplicatorLicense license)
        {
            _context.ApplicatorLicenses.Update(license);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ApplicatorLicenses.FindAsync(id);
            if (entity != null)
            {
                _context.ApplicatorLicenses.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.CustomerBusinessName)
                .ToListAsync();
        }

    }

}
