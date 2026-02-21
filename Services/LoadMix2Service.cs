using PAS.Models;
using PAS.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Npgsql;
using System.Text;

namespace PAS.Services
{
    public class LoadMix2Service
    {
        private readonly AppDbContext _context;
        private readonly IJSRuntime _jsRuntime; // Inject JSRuntime
        private readonly IWebHostEnvironment _env;

        public LoadMix2Service(AppDbContext context, IJSRuntime jsRuntime, IWebHostEnvironment env)
        {
            _context = context;
            _jsRuntime = jsRuntime; // Assign JSRuntime
            _env = env;
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
            // Fetch the parent LoadMix to get the group id
            var loadMix = await _context.LoadMixes
                .Where(lm => lm.Id == loadField.LoadMixId)
                .FirstOrDefaultAsync();

            if (loadMix != null)
            {
                // Set GroupId from LoadMix.LoadMixId
                loadField.GroupId = loadMix.LoadMixId;
            }

            _context.LoadFields.Add(loadField);
            await _context.SaveChangesAsync();
        }


        // Method for updating LoadFields
        public async Task UpdateLoadFieldAsync(LoadFields loadField)
        {
            _context.LoadFields.Update(loadField);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLoadFieldAsync(int id)
        {
            var loadField = await _context.LoadFields.FindAsync(id);
            if (loadField != null)
            {
                _context.LoadFields.Remove(loadField);
                await _context.SaveChangesAsync();
            }
        }

        // Method for getting LoadField by Id
        public async Task<LoadFields> GetLoadFieldByIdAsync(int id)
        {
            return await _context.LoadFields.FirstOrDefaultAsync(lf => lf.Id == id);
        }

        public async Task<List<LoadFields>> GetLoadFieldsByLoadMixIdAsync(int loadMixId)
        {
            return await _context.LoadFields.Where(lf => lf.LoadMixId == loadMixId).ToListAsync();
        }

        // Method for saving LoadMixDetails
        public async Task AddLoadMixDetailsAsync(LoadMixDetails loadMixDetails)
        {
            // Fetch the LoadMix to get the QuoteId
            var loadMix = await _context.LoadMixes
                .Where(lm => lm.Id == loadMixDetails.LoadMixId)
                .FirstOrDefaultAsync();

            if (loadMix != null)
            {
                // Set GroupId from LoadMix.LoadMixId
                loadMixDetails.GroupId = loadMix.LoadMixId;

                // Fetch the correct quote price for the inventory item from QuoteInventories
                var correctQuotePrice = await _context.QuoteInventories
                    .Where(qi => qi.QuoteId == loadMix.QuoteId && qi.ChemicalName == loadMixDetails.Product)  // Ensure it matches the right quote and product
                    .Select(qi => qi.QuotePrice)
                    .FirstOrDefaultAsync();

                // Update the QuotePrice before saving
                loadMixDetails.QuotePrice = correctQuotePrice;
            }

            await _jsRuntime.InvokeVoidAsync("console.log", $"Saving LoadMixDetails: Product: {loadMixDetails.Product}, Corrected QuotePrice: {loadMixDetails.QuotePrice}, LoadMixId: {loadMixDetails.LoadMixId}");

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

        public async Task<bool> IsValidCustomerAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.Id == customerId);
        }

        // Method to generate a unique Load Mix ID
        public async Task<int> GenerateNewLoadMixId()
        {
            //var maxLoadMixId = await _context.LoadMixes.MaxAsync(lm => (int?)lm.Id) ?? 0;
            //return maxLoadMixId + 1;
            var maxGroupId = await _context.LoadMixes.MaxAsync(lm => (int?)lm.LoadMixId) ?? 0;
            return maxGroupId == 0 ? 1000 : maxGroupId + 1;
        }
        public async Task<List<LoadMix>> GetLoadMixesByLoadMixIdAsync(int loadMixId)
        {
            return await _context.LoadMixes
                                 .Include(lm => lm.Quote)
                                 .Include(lm => lm.LoadFields)
                                 .Include(lm => lm.LoadMixDetails) // Include LoadMixDetails
                                 .Where(lm => lm.LoadMixId == loadMixId)
                                 .ToListAsync();
        }

        public async Task<string> BuildLoadMixPrintHtmlAsync(int loadMixId)
        {
            var loadMixes = await GetLoadMixesByLoadMixIdAsync(loadMixId);
            if (loadMixes == null || !loadMixes.Any())
                return "<h3>Load Mix not found.</h3>";

            var first = loadMixes.First();

            var templatePath = Path.Combine(_env.WebRootPath, "print_loadmixtemplate.html");
            var template = await File.ReadAllTextAsync(templatePath);

            // Load Mix rows — only from the first mix to avoid duplication
            var loadMixRows = new StringBuilder();
            foreach (var detail in first.LoadMixDetails)
            {
                loadMixRows.AppendLine($@"
            <tr>
                <td>{detail.Product}</td>
                <td>{detail.RatePerAcre}</td>
                <td>{detail.Total}</td>
            </tr>");
            }

            // Fields Applied To rows
            var fields = await _context.LoadFields
                .Where(f => f.GroupId == loadMixId)
                .ToListAsync();

            var fieldsRows = new StringBuilder();
            foreach (var f in fields)
            {
                fieldsRows.AppendLine($@"
            <tr>
                <td>{f.FieldName}</td>
                <td>{f.FieldAverageRate}</td>
                <td>{f.FieldTotalGallons}</td>
                <td>{f.FieldAcres}</td>
            </tr>");
            }

            template = template
                .Replace("{{LoadId}}", loadMixId.ToString())
                .Replace("{{LoadDate}}", first.LoadDate.ToString("yyyy-MM-dd"))
                .Replace("{{LoadTime}}", first.LoadTime.ToString(@"hh\:mm\:ss"))   // ← FIXED
                .Replace("{{Crop}}", first.Crop)
                .Replace("{{TotalAcres}}", first.TotalAcres.ToString())
                .Replace("{{TotalGallons}}", first.TotalGallons.ToString())
                .Replace("{{RatePerAcre}}", first.LMRatePerAcre.ToString())
                .Replace("{{LoadMixRows}}", loadMixRows.ToString())
                .Replace("{{FieldsAppliedToRows}}", fieldsRows.ToString());

            return template;
        }

    }
}