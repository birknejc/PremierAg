using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PAS.Services
{
    public class QuoteService
    {
        private readonly AppDbContext _context;

        public QuoteService(AppDbContext context)
        {
            _context = context;
        }

        // Fetches a list of all Quotes from the database
        public async Task<List<Quote>> GetQuotesAsync()
        {
            return await _context.Quotes.Include(q => q.Customer) // Include Customer navigation property
                                         .Include(q => q.QuoteInventories) // Include QuoteInventories navigation property
                                         .ThenInclude(qi => qi.Inventory) // Include Inventory navigation property in QuoteInventories
                                         .ToListAsync();
        }

        // Fetches a single Quote by its ID
        public async Task<Quote> GetQuoteByIdAsync(int id)
        {
            return await _context.Quotes.Include(q => q.Customer)
                                         .Include(q => q.QuoteInventories)
                                         .ThenInclude(qi => qi.Inventory)
                                         .FirstOrDefaultAsync(q => q.Id == id);
        }

        // Adds a new Quote to the database
        public async Task AddQuoteAsync(Quote quote)
        {
            if (quote.CustomerBusinessName == null)
            {
                var customer = await _context.Customers.FindAsync(quote.CustomerId);
                if (customer != null)
                {
                    quote.CustomerBusinessName = customer.CustomerBusinessName;
                }
            }

            foreach (var quoteInventory in quote.QuoteInventories)
            {
                var inventory = await _context.Inventories.FindAsync(quoteInventory.InventoryId);
                if (inventory != null)
                {
                    quoteInventory.Inventory = inventory;
                    quoteInventory.ChemicalName = inventory.ChemicalName;
                    quoteInventory.RowVersion = quoteInventory.RowVersion ?? Guid.NewGuid().ToByteArray(); // Ensure RowVersion is initialized
                }
            }

            quote.QuoteDate = quote.QuoteDate.ToUniversalTime(); // Ensure date consistency
            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();
        }






        // Updates an existing Quote in the database
        public async Task UpdateQuoteAsync(Quote quote)
        {
            // Retrieve existing quote with related entities
            var existingQuote = await _context.Quotes
                                              .Include(q => q.QuoteInventories)
                                              .ThenInclude(qi => qi.Inventory)
                                              .FirstOrDefaultAsync(q => q.Id == quote.Id);

            if (existingQuote != null)
            {
                // Update basic quote properties
                existingQuote.CustomerBusinessName = quote.CustomerBusinessName;
                existingQuote.QuoteStreet = quote.QuoteStreet;
                existingQuote.QuoteCity = quote.QuoteCity;
                existingQuote.QuoteState = quote.QuoteState;
                existingQuote.QuoteZipcode = quote.QuoteZipcode;
                existingQuote.QuotePhone = quote.QuotePhone;
                existingQuote.QuoteDate = quote.QuoteDate;

                // Separate new and existing inventories
                var newQuoteInventories = quote.QuoteInventories.Where(qi => qi.Id == 0).ToList();
                var existingQuoteInventories = quote.QuoteInventories.Where(qi => qi.Id != 0).ToList();

                // Handle new inventories
                foreach (var newInventory in newQuoteInventories)
                {
                    var inventory = await _context.Inventories.FindAsync(newInventory.InventoryId);
                    if (inventory != null)
                    {
                        // Check if the combination already exists
                        if (!existingQuote.QuoteInventories.Any(qi => qi.InventoryId == newInventory.InventoryId && qi.QuoteId == newInventory.QuoteId))
                        {
                            newInventory.Inventory = inventory;
                            newInventory.ChemicalName = inventory.ChemicalName;
                            existingQuote.QuoteInventories.Add(newInventory);
                            _context.Entry(newInventory).State = EntityState.Added;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Inventory with Id {newInventory.InventoryId} not found.");
                    }
                }

                // Update existing inventories
                foreach (var existingInventory in existingQuoteInventories)
                {
                    var inventory = await _context.Inventories.FindAsync(existingInventory.InventoryId);
                    if (inventory != null)
                    {
                        var existingItem = existingQuote.QuoteInventories
                            .FirstOrDefault(qi => qi.InventoryId == existingInventory.InventoryId && qi.QuoteId == existingInventory.QuoteId);

                        if (existingItem != null)
                        {
                            existingItem.EPA = existingInventory.EPA;
                            existingItem.Price = existingInventory.Price;
                            existingItem.QuantityPerAcre = existingInventory.QuantityPerAcre;
                            existingItem.QuotePrice = existingInventory.QuotePrice;
                            existingItem.QuoteUnitOfMeasure = existingInventory.QuoteUnitOfMeasure;
                            existingItem.UnitOfMeasure = existingInventory.UnitOfMeasure;

                            // Handle concurrency by updating RowVersion
                            _context.Entry(existingItem).OriginalValues["RowVersion"] = existingInventory.RowVersion;
                            _context.Entry(existingItem).State = EntityState.Modified;
                        }
                        else
                        {
                            throw new InvalidOperationException($"QuoteInventory with InventoryId {existingInventory.InventoryId} and QuoteId {existingInventory.QuoteId} not found.");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Inventory with Id {existingInventory.InventoryId} not found.");
                    }
                }

                // Save changes with concurrency handling
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
                                var proposedValues = entry.CurrentValues;
                                var databaseValues = entry.GetDatabaseValues();

                                if (databaseValues != null)
                                {
                                    entry.OriginalValues.SetValues(databaseValues);
                                    entry.CurrentValues.SetValues(proposedValues);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Entity was deleted by another user. Aborting operation.");
                                }
                            }
                        }
                    }
                } while (saveFailed);
            }
        }


        // Deletes a Quote from the database by its ID
        public async Task DeleteQuoteAsync(int id)
        {
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

        // Fetches a list of all Inventories for the lookup in the QuotePage
        public async Task<List<Inventory>> GetInventoriesAsync()
        {
            return await _context.Inventories.ToListAsync();
        }
    }
}
