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
        private readonly ProductInventoryService _productInventoryService;

        public LoadMix2Service(AppDbContext context, IJSRuntime jsRuntime, IWebHostEnvironment env, ProductInventoryService productInventoryService)
        {
            _context = context;
            _jsRuntime = jsRuntime;
            _env = env;
            _productInventoryService = productInventoryService;
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
            if (await LoadMixIsLockedAsync(loadMix.Id)) 
                throw new InvalidOperationException(
                    "This load mix is locked because it has been invoiced or contains field/product data and cannot be edited.");

            _context.LoadMixes.Update(loadMix);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLoadMixAsync(int id)
        {
            // ⭐ Use the parameter name "id" here
            if (await LoadMixIsLockedAsync(id))
                throw new InvalidOperationException(
                    "This load mix is locked because it has been invoiced or contains field/product data and cannot be deleted.");

            var loadMix = await _context.LoadMixes.FindAsync(id);
            if (loadMix != null)
            {
                _context.LoadMixes.Remove(loadMix);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteLoadMixGroupAsync(int groupId)
        {
            // All LoadMixes in this group
            var loadMixes = await _context.LoadMixes
                .Where(lm => lm.LoadMixId == groupId)
                .ToListAsync();

            if (!loadMixes.Any())
                return;

            // Determine primary LoadMix (same rule as Step 2C)
            var primaryLoadMixId = await GetPrimaryLoadMixIdForGroupAsync(groupId);
            LoadMix primaryLoadMix = null;

            if (primaryLoadMixId.HasValue)
            {
                primaryLoadMix = loadMixes.FirstOrDefault(lm => lm.Id == primaryLoadMixId.Value);
            }

            // ⭐ RESTORE INVENTORY ONLY FOR PRIMARY LOADMIX
            if (primaryLoadMix != null)
            {
                var primaryDetails = await _context.LoadMixDetails
                    .Where(d => d.LoadMixId == primaryLoadMix.Id)
                    .ToListAsync();

                foreach (var detail in primaryDetails)
                {
                    if (detail.ProductId.HasValue &&
                        detail.ProductId.Value > 0 &&
                        detail.TotalUsed > 0)
                    {
                        await _productInventoryService.RestoreInventoryAsync(
                            detail.ProductId.Value,
                            detail.TotalUsed,
                            groupId,            // ⭐ required for audit
                            primaryLoadMix.Id,  // ⭐ loadMixId
                            detail.Id           // ⭐ loadMixDetailsId
                        );
                    }
                }
            }

            // Delete all details for all LoadMixes in the group
            var allDetails = await _context.LoadMixDetails
                .Where(d => d.GroupId == groupId)
                .ToListAsync();

            _context.LoadMixDetails.RemoveRange(allDetails);

            // Delete all LoadMixes in the group
            _context.LoadMixes.RemoveRange(loadMixes);

            await _context.SaveChangesAsync();
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
            // Load the parent LoadMix so we know the group and quote
            var loadMix = await _context.LoadMixes
                .FirstOrDefaultAsync(lm => lm.Id == loadMixDetails.LoadMixId);

            if (loadMix != null)
            {
                // Always set GroupId from the parent LoadMix
                loadMixDetails.GroupId = loadMix.LoadMixId;

                // ⭐ Existing QuotePrice matching logic (unchanged)
                QuoteInventory matchingQuote = null;

                if (loadMixDetails.ProductId.HasValue)
                {
                    matchingQuote = await _context.QuoteInventories
                        .Where(qi =>
                            qi.QuoteId == loadMix.QuoteId &&
                            qi.ProductId == loadMixDetails.ProductId)
                        .FirstOrDefaultAsync();
                }

                if (matchingQuote == null && !string.IsNullOrWhiteSpace(loadMixDetails.EPA))
                {
                    matchingQuote = await _context.QuoteInventories
                        .Where(qi =>
                            qi.QuoteId == loadMix.QuoteId &&
                            qi.EPA != null &&
                            qi.EPA.Trim().ToLower() == loadMixDetails.EPA.Trim().ToLower())
                        .FirstOrDefaultAsync();
                }

                if (matchingQuote == null)
                {
                    matchingQuote = await _context.QuoteInventories
                        .Where(qi =>
                            qi.QuoteId == loadMix.QuoteId &&
                            qi.ChemicalName != null &&
                            qi.ChemicalName.Trim().ToLower() ==
                                loadMixDetails.Product.Trim().ToLower())
                        .FirstOrDefaultAsync();
                }

                if (matchingQuote != null)
                {
                    loadMixDetails.QuotePrice = matchingQuote.QuotePrice;
                }
            }

            await _jsRuntime.InvokeVoidAsync("console.log",
                $"Saving LoadMixDetails: Product={loadMixDetails.Product}, " +
                $"ProductId={loadMixDetails.ProductId}, " +
                $"TotalUsed={loadMixDetails.TotalUsed}, " +
                $"LoadMixId={loadMixDetails.LoadMixId}, " +
                $"GroupId={loadMixDetails.GroupId}");

            // Save the detail row
            _context.LoadMixDetails.Add(loadMixDetails);
            await _context.SaveChangesAsync();

            // ⭐ GROUPID‑AWARE INVENTORY DEDUCTION ⭐

            // 1️⃣ If no ProductId, nothing to deduct (e.g., Water)
            if (!loadMixDetails.ProductId.HasValue || loadMixDetails.ProductId.Value <= 0)
                return;

            // 2️⃣ If no parent LoadMix, bail out safely
            if (loadMix == null)
                return;

            // 3️⃣ Determine the primary LoadMix for this group
            var primaryLoadMixId = await GetPrimaryLoadMixIdForGroupAsync(loadMix.LoadMixId);
            if (!primaryLoadMixId.HasValue)
                return;

            // 4️⃣ Only deduct inventory if THIS detail belongs to the primary LoadMix
            if (loadMix.Id != primaryLoadMixId.Value)
                return;

            // 5️⃣ Finally, deduct inventory using FIFO based on TotalUsed
            if (loadMixDetails.TotalUsed > 0)
            {
                await _productInventoryService.ConsumeInventoryAsync(
                    loadMixDetails.ProductId.Value,
                    loadMixDetails.TotalUsed,
                    loadMixDetails.GroupId,       // ⭐ groupId
                    loadMixDetails.LoadMixId,     // ⭐ loadMixId
                    loadMixDetails.Id             // ⭐ loadMixDetailsId
                );
            }
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

        private async Task<bool> LoadMixIsLockedAsync(int loadMixId)
        {
            var loadMix = await _context.LoadMixes
                .FirstOrDefaultAsync(lm => lm.Id == loadMixId);

            if (loadMix == null)
                return false;

            // ⭐ Only invoices lock the LoadMix
            if (loadMix.QuoteId.HasValue)
            {
                bool invoiced = await _context.Invoices
                    .AnyAsync(i => i.QuoteId == loadMix.QuoteId.Value);

                if (invoiced)
                    return true;
            }

            return false;
        }

        private async Task<int?> GetPrimaryLoadMixIdForGroupAsync(int groupId)
        {
            // All LoadMixes in this group
            var loadMixes = await _context.LoadMixes
                .Where(lm => lm.LoadMixId == groupId)
                .ToListAsync();

            if (!loadMixes.Any())
                return null;

            // 1️⃣ Prefer those with QuoteId, pick lowest Id
            var withQuote = loadMixes
                .Where(lm => lm.QuoteId.HasValue)
                .OrderBy(lm => lm.Id)
                .FirstOrDefault();

            if (withQuote != null)
                return withQuote.Id;

            // 2️⃣ Otherwise, pick the No‑Quote LoadMix with lowest Id
            var noQuote = loadMixes
                .Where(lm => !lm.QuoteId.HasValue)
                .OrderBy(lm => lm.Id)
                .FirstOrDefault();

            return noQuote?.Id;
        }

        public async Task UpdateLoadMixDetailsGroupAwareAsync(int loadMixId, List<LoadMixDetails> newDetails)
        {
            // Load parent LoadMix
            var loadMix = await _context.LoadMixes
                .FirstOrDefaultAsync(lm => lm.Id == loadMixId);

            if (loadMix == null)
                throw new Exception("LoadMix not found.");

            int groupId = loadMix.LoadMixId;

            // Determine primary LoadMix
            var primaryLoadMixId = await GetPrimaryLoadMixIdForGroupAsync(groupId);
            bool isPrimary = (primaryLoadMixId.HasValue && primaryLoadMixId.Value == loadMixId);

            // Load old details
            var oldDetails = await _context.LoadMixDetails
                .Where(d => d.LoadMixId == loadMixId)
                .ToListAsync();

            // ⭐ If this is the primary LoadMix, restore inventory for old details
            if (isPrimary)
            {
                foreach (var detail in oldDetails)
                {
                    if (detail.ProductId.HasValue &&
                        detail.ProductId.Value > 0 &&
                        detail.TotalUsed > 0)
                    {
                        await _productInventoryService.RestoreInventoryAsync(
                            detail.ProductId.Value,
                            detail.TotalUsed,
                            groupId,        // ⭐ required for audit
                            loadMixId,      // ⭐ loadMixId
                            detail.Id       // ⭐ loadMixDetailsId
                        );
                    }
                }
            }

            // Delete old details
            _context.LoadMixDetails.RemoveRange(oldDetails);
            await _context.SaveChangesAsync();

            // Insert new details
            foreach (var detail in newDetails)
            {
                detail.LoadMixId = loadMixId;
                detail.GroupId = groupId;

                // This will automatically:
                // - save the detail
                // - deduct inventory if this is the primary LoadMix
                await AddLoadMixDetailsAsync(detail);
            }
        }


    }
}