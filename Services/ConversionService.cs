using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class ConversionService
    {
        private readonly AppDbContext _context;

        public ConversionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UOMConversion>> GetAllConversionsAsync()
        {
            return await _context.UOMConversions.ToListAsync();
        }

        public async Task<UOMConversion> GetConversionByIdAsync(int id)
        {
            return await _context.UOMConversions.FindAsync(id);
        }

        public async Task AddConversionAsync(UOMConversion conversion)
        {
            _context.UOMConversions.Add(conversion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConversionAsync(UOMConversion conversion)
        {
            var existingConversion = await _context.UOMConversions.FindAsync(conversion.Id);
            if (existingConversion != null)
            {
                existingConversion.PUOM = conversion.PUOM;
                existingConversion.SUOM = conversion.SUOM;
                existingConversion.QUOM = conversion.QUOM;
                existingConversion.ConversionFactor = conversion.ConversionFactor;
                existingConversion.CFPurSold = conversion.CFPurSold; // Update CFPurSold
                _context.UOMConversions.Update(existingConversion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteConversionAsync(int id)
        {
            var conversion = await _context.UOMConversions.FindAsync(id);
            if (conversion != null)
            {
                _context.UOMConversions.Remove(conversion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
