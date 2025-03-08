using PAS.Models;
using PAS.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace PAS.Services
{
    public class LoadMixService
    {
        private readonly AppDbContext _context;
        private readonly IJSRuntime _jsRuntime; // Inject JSRuntime

        public LoadMixService(AppDbContext context, IJSRuntime jsRuntime)
        {
            _context = context;
            _jsRuntime = jsRuntime; // Assign JSRuntime
        }

        public async Task<List<LoadMix>> GetAllLoadMixesAsync()
        {
            return await _context.LoadMixes.Include(lm => lm.Quote).ToListAsync();
        }

        public async Task<LoadMix> GetLoadMixByIdAsync(int id)
        {
            return await _context.LoadMixes.Include(lm => lm.Quote)
                                           .Include(lm => lm.LoadFields) // Include LoadFields
                                           .FirstOrDefaultAsync(lm => lm.Id == id);
        }

        public async Task<List<LoadMixDetails>> GetLoadMixDetailsByLoadMixIdAsync(int loadMixId)
        {
            var loadMixDetails = await _context.LoadMixDetails.Where(lmd => lmd.LoadMixId == loadMixId).ToListAsync();
            return loadMixDetails;
        }

        public async Task<LoadMixDetails> GetLoadMixDetailsByIdAsync(int id)
        {
            return await _context.LoadMixDetails.FirstOrDefaultAsync(lmd => lmd.Id == id);
        }

        public async Task AddLoadMixAsync(LoadMix loadMix)
        {
            _context.LoadMixes.Add(loadMix);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLoadMixAsync(LoadMix loadMix)
        {
            _context.LoadMixes.Update(loadMix);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLoadMixAsync(int id)
        {
            var loadMix = await _context.LoadMixes.FindAsync(id);
            if (loadMix != null)
            {
                _context.LoadMixes.Remove(loadMix);
                await _context.SaveChangesAsync();
            }
        }

        // Method for saving LoadFields
        public async Task AddLoadFieldAsync(LoadFields loadField)
        {
            _context.LoadFields.Add(loadField);
            await _context.SaveChangesAsync();
        }

        // Method for updating LoadFields
        public async Task UpdateLoadFieldAsync(LoadFields loadField)
        {
            _context.LoadFields.Update(loadField);
            await _context.SaveChangesAsync();
        }

        // Method for getting LoadField by Id
        public async Task<LoadFields> GetLoadFieldByIdAsync(int id)
        {
            return await _context.LoadFields.FirstOrDefaultAsync(lf => lf.Id == id);
        }

        // Method for saving LoadMixDetails
        public async Task AddLoadMixDetailsAsync(LoadMixDetails loadMixDetails)
        {
            _context.LoadMixDetails.Add(loadMixDetails);
            await _context.SaveChangesAsync();
        }

        // Method for updating LoadMixDetails
        public async Task UpdateLoadMixDetailsAsync(LoadMixDetails loadMixDetails)
        {
            _context.LoadMixDetails.Update(loadMixDetails);
            await _context.SaveChangesAsync();
        }

        public async Task<LoadMix> GetLoadMixWithDetailsAndFieldsByIdAsync(int id)
        {
            return await _context.LoadMixes
                                 .Include(lm => lm.Quote)
                                 .Include(lm => lm.LoadMixDetails)  // Include LoadMixDetails
                                 .Include(lm => lm.LoadFields)      // Include LoadFields
                                 .FirstOrDefaultAsync(lm => lm.Id == id);
        }

        public async Task RemoveLoadMixDetailsAsync(int loadMixId, string product)
        {
            var loadMixDetails = await _context.LoadMixDetails
                                                .Where(lmd => lmd.LoadMixId == loadMixId && lmd.Product == product)
                                                .ToListAsync();

            if (loadMixDetails != null && loadMixDetails.Any())
            {
                _context.LoadMixDetails.RemoveRange(loadMixDetails);
                await _context.SaveChangesAsync();
            }
        }

    }
}
