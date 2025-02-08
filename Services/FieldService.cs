using PAS.Models;
using PAS.DBContext;
using Microsoft.EntityFrameworkCore;

namespace PAS.Services
{
    public class FieldService
    {
        private readonly AppDbContext _context;

        public FieldService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Field>> GetAllFieldsAsync()
        {
            return await _context.Fields.Include(f => f.Customer).ToListAsync();
        }

        public async Task<List<Field>> GetFieldsByCustomerIdAsync(int customerId)
        {
            return await _context.Fields
                                 .Where(f => f.CustomerId == customerId)
                                 .ToListAsync();
        }

        public async Task<Field> GetFieldByIdAsync(int id)
        {
            return await _context.Fields.Include(f => f.Customer)
                                         .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFieldAsync(Field field)
        {
            _context.Fields.Add(field);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFieldAsync(Field field)
        {
            _context.Fields.Update(field);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFieldAsync(int id)
        {
            var field = await _context.Fields.FindAsync(id);
            if (field != null)
            {
                _context.Fields.Remove(field);
                await _context.SaveChangesAsync();
            }
        }
    }
}
