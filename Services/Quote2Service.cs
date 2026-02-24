using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Text;

namespace PAS.Services
{
    public class Quote2Service
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public Quote2Service(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<Quote>> GetQuotesAsync()
        {
            var quotes = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteInventories)
                    .ThenInclude(qi => qi.Product)
                .ToListAsync();

            foreach (var q in quotes)
                await AutoExpireQuoteAsync(q);

            return quotes;
        }




        public async Task<Quote> GetQuoteByIdAsync(int id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteInventories)
                    .ThenInclude(qi => qi.Product)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote != null) await AutoExpireQuoteAsync(quote);

            if (quote == null)
                return null;

            // ⭐ HYDRATE HERE — BEFORE DETACHING
            foreach (var qi in quote.QuoteInventories)
            {
                if (qi.Product != null)
                {
                    qi.ProductName = qi.Product.Name;
                    qi.ProductEPA = qi.Product.EPA;
                    qi.ProductUOM = qi.Product.DefaultUnitOfMeasure;
                    qi.ProductWeightedAveragePrice = qi.Product.WeightedAveragePrice;

                    // Legacy compatibility (optional)
                    qi.ChemicalName = qi.Product.Name;
                    qi.EPA = qi.Product.EPA;
                    qi.UnitOfMeasure = qi.Product.DefaultUnitOfMeasure;
                    qi.Price = qi.Product.WeightedAveragePrice;
                }
            }

            // ⭐ NOW DETACH
            _context.Entry(quote).State = EntityState.Detached;

            foreach (var qi in quote.QuoteInventories)
            {
                _context.Entry(qi).State = EntityState.Detached;

                if (qi.Product != null)
                    _context.Entry(qi.Product).State = EntityState.Detached;
            }

            return quote;
        }

        // Adds a new Quote to the database
        public async Task AddQuoteAsync(Quote quote)
        {
            // Ensure CustomerBusinessName is populated
            if (quote.CustomerBusinessName == null)
            {
                var customer = await _context.Customers.FindAsync(quote.CustomerId);
                if (customer != null)
                {
                    quote.CustomerBusinessName = customer.CustomerBusinessName;
                }
            }

            // Hydrate product snapshot fields
            foreach (var qi in quote.QuoteInventories)
            {
                if (qi.ProductId.HasValue)
                {
                    var product = await _context.Products.FindAsync(qi.ProductId.Value);
                    if (product != null)
                    {
                        qi.ProductId = product.Id;
                        qi.Product = product;
                        qi.ProductName = product.Name;
                        qi.ProductEPA = product.EPA;
                        qi.ProductUOM = product.DefaultUnitOfMeasure;
                        qi.ProductWeightedAveragePrice = product.WeightedAveragePrice;

                        // Legacy compatibility fields
                        qi.ChemicalName = product.Name;
                        qi.EPA = product.EPA;
                        qi.UnitOfMeasure = product.DefaultUnitOfMeasure;
                        qi.Price = product.WeightedAveragePrice;

                        // Concurrency token
                        qi.RowVersion ??= Guid.NewGuid().ToByteArray();
                    }
                }
            }

            // Totals
            quote.QuoteTotal = quote.QuoteInventories.Sum(qi => qi.QuotePrice);

            // ⭐ Normalize QuoteDate to UTC
            quote.QuoteDate = DateTime.SpecifyKind(quote.QuoteDate, DateTimeKind.Utc);

            // ⭐ Normalize ArchiveDate to UTC
            quote.ArchiveDate = DateTime.SpecifyKind(quote.ArchiveDate, DateTimeKind.Utc);

            // Save
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
        }


        // Updates an existing Quote in the database
        public async Task UpdateQuoteAsync(Quote quote)
        {
            if (await QuoteIsLockedAsync(quote.Id))
                throw new InvalidOperationException(
                    "This quote is locked because it has been used in a load mix or invoice and cannot be edited.");

            // Load existing quote with Product-based inventories only
            var existingQuote = await _context.Quotes
                .Include(q => q.QuoteInventories)
                .ThenInclude(qi => qi.Product)
                .FirstOrDefaultAsync(q => q.Id == quote.Id);

            if (existingQuote == null)
                throw new InvalidOperationException($"Quote with Id {quote.Id} not found.");

            //
            // Update quote-level fields
            //
            existingQuote.CustomerBusinessName = quote.CustomerBusinessName;
            existingQuote.QuoteStreet = quote.QuoteStreet;
            existingQuote.QuoteCity = quote.QuoteCity;
            existingQuote.QuoteState = quote.QuoteState;
            existingQuote.QuoteZipcode = quote.QuoteZipcode;
            existingQuote.QuotePhone = quote.QuotePhone;

            // ⭐ Normalize QuoteDate to UTC
            existingQuote.QuoteDate = DateTime.SpecifyKind(quote.QuoteDate, DateTimeKind.Utc);

            // ⭐ Normalize ArchiveDate to UTC
            existingQuote.ArchiveDate = DateTime.SpecifyKind(quote.ArchiveDate, DateTimeKind.Utc);

            //
            // Separate new vs existing QuoteInventories
            //
            var newItems = quote.QuoteInventories.Where(qi => qi.Id == 0).ToList();
            var updatedItems = quote.QuoteInventories.Where(qi => qi.Id != 0).ToList();

            //
            // 1. Add NEW Product-based QuoteInventories
            //
            foreach (var newItem in newItems)
            {
                if (!newItem.ProductId.HasValue)
                    throw new InvalidOperationException("ProductId is required for Quote2.");

                var product = await _context.Products.FindAsync(newItem.ProductId.Value);
                if (product == null)
                    throw new InvalidOperationException($"Product with Id {newItem.ProductId} not found.");

                // Product snapshot fields
                newItem.Product = product;
                newItem.ProductName = product.Name;
                newItem.ProductEPA = product.EPA;
                newItem.ProductUOM = product.DefaultUnitOfMeasure;
                newItem.ProductWeightedAveragePrice = product.WeightedAveragePrice;

                // Legacy compatibility fields
                newItem.ChemicalName = product.Name;
                newItem.EPA = product.EPA;
                newItem.UnitOfMeasure = product.DefaultUnitOfMeasure;
                newItem.Price = product.WeightedAveragePrice;

                // Concurrency token
                newItem.RowVersion ??= Guid.NewGuid().ToByteArray();

                existingQuote.QuoteInventories.Add(newItem);
                _context.Entry(newItem).State = EntityState.Added;
            }

            //
            // 2. Update EXISTING Product-based QuoteInventories
            //
            foreach (var updatedItem in updatedItems)
            {
                var existingItem = existingQuote.QuoteInventories
                    .FirstOrDefault(qi => qi.Id == updatedItem.Id);

                if (existingItem == null)
                    throw new InvalidOperationException($"QuoteInventory with Id {updatedItem.Id} not found.");

                // Reload product
                var product = await _context.Products.FindAsync(updatedItem.ProductId.Value);
                if (product == null)
                    throw new InvalidOperationException($"Product with Id {updatedItem.ProductId} not found.");

                // Product snapshot fields
                existingItem.ProductId = product.Id;
                existingItem.Product = product;
                existingItem.ProductName = product.Name;
                existingItem.ProductEPA = product.EPA;
                existingItem.ProductUOM = product.DefaultUnitOfMeasure;
                existingItem.ProductWeightedAveragePrice = product.WeightedAveragePrice;

                // Legacy compatibility fields
                existingItem.ChemicalName = product.Name;
                existingItem.EPA = product.EPA;
                existingItem.UnitOfMeasure = product.DefaultUnitOfMeasure;
                existingItem.Price = product.WeightedAveragePrice;

                // Quote-specific fields
                existingItem.QuantityPerAcre = updatedItem.QuantityPerAcre;
                existingItem.QuotePrice = updatedItem.QuotePrice;
                existingItem.QuoteUnitOfMeasure = updatedItem.QuoteUnitOfMeasure;

                // Concurrency
                _context.Entry(existingItem).OriginalValues["RowVersion"] = updatedItem.RowVersion;
                _context.Entry(existingItem).State = EntityState.Modified;
            }

            //
            // Recalculate total
            //
            existingQuote.QuoteTotal = existingQuote.QuoteInventories.Sum(qi => qi.QuotePrice);

            //
            // Save with concurrency handling
            //
            bool saveFailed;
            do
            {
                saveFailed = false;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    foreach (var entry in ex.Entries)
                    {
                        if (entry.Entity is QuoteInventory)
                        {
                            var proposed = entry.CurrentValues;
                            var database = entry.GetDatabaseValues();

                            if (database != null)
                            {
                                entry.OriginalValues.SetValues(database);
                                entry.CurrentValues.SetValues(proposed);
                            }
                            else
                            {
                                throw new InvalidOperationException("QuoteInventory was deleted by another user.");
                            }
                        }
                    }
                }
            } while (saveFailed);
        }



        // Deletes a Quote from the database by its ID
        public async Task DeleteQuoteAsync(int id)
        {
            if (await QuoteIsLockedAsync(id)) 
                throw new InvalidOperationException(
                    "This quote is locked because it has been used in a load mix or invoice and cannot be deleted.");

            var quote = await _context.Quotes.FindAsync(id);
            if (quote != null)
            {
                _context.Quotes.Remove(quote);
                await _context.SaveChangesAsync();
            }
        }

        // Fetches a list of all Customers for the lookup in the QuotePage
        public async Task<List<Customer>> GetCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        // Fetches a list of fields based on the CustomerId
        public async Task<List<Field>> GetFieldsByCustomerIdAsync(int customerId)
        {
            return await _context.Fields
                                 .Where(f => f.CustomerId == customerId)
                                 .ToListAsync();
        }

        // Fetches a list of all Fields for the lookup in the QuotePage
        public async Task<List<Field>> GetFieldsAsync()
        {
            return await _context.Fields.ToListAsync();
        }

        public async Task<string> BuildQuotePrintHtmlAsync(int quoteId)
        {
            // Load the quote with inventories
            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.QuoteInventories)
                .FirstOrDefaultAsync(q => q.Id == quoteId);

            if (quote == null)
                return "<h3>Quote not found.</h3>";

            // Load the template
            var templatePath = Path.Combine(_env.WebRootPath, "print_quotetemplate.html");
            var template = await File.ReadAllTextAsync(templatePath);

            // Build table rows
            var rows = new StringBuilder();

            foreach (var item in quote.QuoteInventories.OrderBy(q => q.ChemicalName))
            {
                rows.AppendLine($@"
            <tr>
                <td>{item.ChemicalName}</td>
                <td>{item.QuantityPerAcre} {item.QuoteUnitOfMeasure}</td>
                <td>{item.UnitOfMeasure}</td>
                <td>{item.QuotePrice.ToString("C")}</td>
            </tr>");
            }

            // Build the full quote content
            var quoteContent = $@"
        <div class='quote-header'>
            <img src='/images/logo.jpg' class='logo' />
            <h2>Quote #{quote.Id}</h2>
        </div>

        <div class='content-row'>
            <div class='content-column1'>
                <p><strong>Customer:</strong> {quote.Customer.CustomerBusinessName}</p>
                <p><strong>Date:</strong> {quote.QuoteDate.ToString("MM/dd/yyyy")}</p>
            </div>
            <div class='content-column2'>
                <p><strong>Prepared By:</strong><br/>Premier Ag Solution, LLC</p>
                <p>17564 Nuthatch Ave<br/>Bloomfield, IA 52537</p>
                <p>641‑777‑5997 / 641‑777‑2968</p>
            </div>
        </div>

        <table>
            <thead>
                <tr>
                    <th>Product</th>
                    <th>Rate / Acre</th>
                    <th>Unit of Measure</th>
                    <th>Quote Price</th>
                </tr>
            </thead>
            <tbody>
                {rows}
            </tbody>
        </table>

        <div class='total-amount'>
            <strong>Total Quote Amount:</strong> {quote.QuoteInventories.Sum(q => q.QuotePrice).ToString("C")}
        </div>
    ";

            // Inject into template
            template = template
                .Replace("{{QuotesContent}}", quoteContent)
                .Replace("{{QuoteId}}", quote.Id.ToString())
                .Replace("{{QuoteDate}}", quote.QuoteDate.ToString("MM/dd/yyyy"))
                .Replace("{{CustomerName}}", quote.Customer.CustomerBusinessName)
                .Replace("{{TotalAmount}}", quote.QuoteInventories.Sum(q => q.QuotePrice).ToString("C"));

            return template;
        }

        private async Task<bool> QuoteIsLockedAsync(int quoteId)
        {
            // 1️⃣ Any LoadMix referencing this Quote?
            var loadMixes = await _context.LoadMixes
                .Where(lm => lm.QuoteId == quoteId)
                .ToListAsync();

            if (!loadMixes.Any())
                return false; // Quote is free to edit/delete

            // 2️⃣ Any LoadFields under those LoadMixes?
            var loadMixIds = loadMixes.Select(lm => lm.Id).ToList();

            bool hasFields = await _context.LoadFields
                .AnyAsync(f => loadMixIds.Contains(f.LoadMixId));

            if (hasFields)
                return true;

            // 3️⃣ Any Invoices referencing this Quote?
            bool invoiced = await _context.Invoices
                .AnyAsync(i => i.QuoteId == quoteId);

            if (invoiced)
                return true;

            return false;
        }

        public async Task AutoExpireQuoteAsync(Quote quote)
        {
            if (quote.Status == QuoteStatus.Active &&
                DateTime.Today >= quote.ArchiveDate)
            {
                quote.Status = QuoteStatus.Expired;
                await _context.SaveChangesAsync();
            }
        }

    }
}
