//using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PAS.Models;
using PAS.DBContext;

namespace PAS.Services
{
    public class InvoiceService
    {
        private readonly AppDbContext _context;
        //private readonly IJSRuntime _jsRuntime;

        public InvoiceService(AppDbContext context)//, IJSRuntime jsRuntime)
        {
            _context = context;
            //_jsRuntime = jsRuntime; // Initialize JSRuntime
        }

        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.QuoteInventory)
                .Where(i => !i.IsPrinted) // Only fetch invoices that are not printed
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.QuoteInventory)
                .ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.QuoteInventory)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<(List<Invoice> InvoicesWithQuote, List<NoQuoteInvoice> InvoicesWithoutQuote)> GenerateInvoicesAsync()
        {
            var invoicesWithQuote = await GenerateInvoicesWithQuoteIdAsync();
            var invoicesWithoutQuote = await GenerateInvoicesWithoutQuoteAsync();

            return (invoicesWithQuote, invoicesWithoutQuote);
        }

        private async Task<List<Invoice>> GenerateInvoicesWithQuoteIdAsync()
        {
            var invoices = new List<Invoice>();
            var existingInvoices = new HashSet<string>();

            var quotes = await _context.QuoteInventories
                .Include(qi => qi.Quote)
                .Include(qi => qi.Quote.Customer)
                .ToListAsync();

            var loadMixes = await _context.LoadMixes
                .ToListAsync();

            foreach (var quote in quotes)
            {
                if (quote.ChemicalName == "Water")
                {
                    continue;
                }

                var loadMix = loadMixes.FirstOrDefault(lm => lm.QuoteId == quote.QuoteId);
                if (loadMix != null)
                {
                    string inventoryUnitOfMeasure = quote.UnitOfMeasure;

                    var invoiceKey = $"{quote.QuoteId}-{quote.InventoryId}-{quote.Quote.Customer.Id}-{quote.ChemicalName}";

                    if (!existingInvoices.Contains(invoiceKey))
                    {
                        var invoice = new Invoice
                        {
                            InvoiceCustomer = quote.Quote.Customer.CustomerBusinessName,
                            InvoiceChemicalName = quote.ChemicalName,
                            InvoiceUnitOfMeasure = quote.QuoteUnitOfMeasure,
                            UnitOfMeasure = quote.UnitOfMeasure,
                            InvoicePrice = quote.QuotePrice,
                            QuoteId = quote.QuoteId,
                            InventoryId = quote.InventoryId,
                            CustomerId = quote.Quote.Customer.Id,
                            InvoiceRatePerAcre = 0,
                            IsPrinted = false
                        };

                        invoices.Add(invoice);
                        existingInvoices.Add(invoiceKey);

                        var loadMixDetails = await _context.LoadMixDetails
                            .Where(lmd => lmd.LoadMixId == loadMix.Id && lmd.Product == invoice.InvoiceChemicalName)
                            .ToListAsync();

                        foreach (var detail in loadMixDetails)
                        {
                            decimal ratePerAcre = ExtractRatePerAcre(detail.RatePerAcre);

                            var loadFields = await _context.LoadFields
                                .Where(lf => lf.LoadMixId == loadMix.Id)
                                .ToListAsync();

                            foreach (var field in loadFields)
                            {
                                var conversionFactor = await _context.UOMConversions
                                    .FirstOrDefaultAsync(uom => uom.QUOM == invoice.InvoiceUnitOfMeasure && uom.SUOM == inventoryUnitOfMeasure);

                                if (conversionFactor != null)
                                {
                                    decimal newRatePerAcre = (ratePerAcre * field.FieldAcres) / conversionFactor.ConversionFactor;
                                    invoice.InvoiceRatePerAcre += newRatePerAcre;
                                }
                            }
                        }

                        invoice.InvoiceRatePerAcre = Math.Round(invoice.InvoiceRatePerAcre, 2);
                    }
                }
            }

            return invoices;
        }

        private async Task<List<NoQuoteInvoice>> GenerateInvoicesWithoutQuoteAsync()
        {
            var invoices = new List<NoQuoteInvoice>();
            var existingInvoices = new HashSet<string>();

            var loadMixes = await _context.LoadMixes
                .Where(lm => lm.QuoteId == null)
                .ToListAsync();

            foreach (var loadMix in loadMixes)
            {
                // New logic for NoQuote scenario

                var loadMixDetails = await _context.LoadMixDetails
                    .Where(lmd => lmd.LoadMixId == loadMix.Id)
                    .ToListAsync();

                if (loadMixDetails.Count > 0)
                {
                    var invoiceKey = $"{loadMix.Id}-{loadMixDetails.First().Product}";

                    if (!existingInvoices.Contains(invoiceKey))
                    {
                        var loadFields = await _context.LoadFields
                            .Where(lf => lf.LoadMixId == loadMix.Id)
                            .Include(lf => lf.LoadMix)
                            .ToListAsync();

                        var firstField = loadFields.FirstOrDefault();
                        Customer customer = null;

                        if (firstField != null)
                        {
                            var field = await _context.Fields
                                .Include(f => f.Customer)
                                .FirstOrDefaultAsync(f => f.FieldName == firstField.FieldName);

                            customer = field?.Customer;
                        }

                        var invoice = new NoQuoteInvoice
                        {
                            InvoiceCustomer = customer?.CustomerBusinessName,
                            InvoiceChemicalName = loadMixDetails.First().Product,
                            InvoiceUnitOfMeasure = loadMixDetails.First().QuoteUnitOfMeasure,
                            UnitOfMeasure = loadMixDetails.First().QuoteUnitOfMeasure,
                            InvoicePrice = loadMixDetails.First().Price ?? 0,
                            CustomerId = customer?.Id ?? 0,
                            InvoiceRatePerAcre = 0,
                            IsPrinted = false
                        };

                        invoices.Add(invoice);
                        existingInvoices.Add(invoiceKey);

                        foreach (var field in loadFields)
                        {
                            var conversionFactor = await _context.UOMConversions
                                .FirstOrDefaultAsync(uom => uom.QUOM == invoice.InvoiceUnitOfMeasure && uom.SUOM == loadMixDetails.First().QuoteUnitOfMeasure);

                            if (conversionFactor != null)
                            {
                                decimal newRatePerAcre = (loadMix.LMRatePerAcre * field.FieldAcres) / conversionFactor.ConversionFactor;
                                invoice.InvoiceRatePerAcre += newRatePerAcre;

                            }
                        }

                        invoice.InvoiceRatePerAcre = Math.Round(invoice.InvoiceRatePerAcre, 2);
                    }
                }
            }

            return invoices;
        }

        private decimal ExtractRatePerAcre(string ratePerAcre)
        {
            var match = System.Text.RegularExpressions.Regex.Match(ratePerAcre, @"\d+(\.\d+)?");
            if (match.Success)
            {
                return decimal.Parse(match.Value);
            }
            return 0;
        }

        public async Task AddInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task AddNoQuoteInvoiceAsync(NoQuoteInvoice invoice)
        {
            _context.NoQuoteInvoices.Add(invoice);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteNoQuoteInvoiceAsync(int id)
        {
            // Find the NoQuoteInvoice record by Id
            var noQuoteInvoice = await _context.NoQuoteInvoices.FindAsync(id);

            if (noQuoteInvoice != null)
            {
                // Remove the record if it exists
                _context.NoQuoteInvoices.Remove(noQuoteInvoice);
                await _context.SaveChangesAsync();
            }
        }
        public async Task MarkInvoicesAsPrintedAsync(List<int> invoiceIds)
        {
            var invoices = await _context.Invoices
                .Where(i => invoiceIds.Contains(i.Id))
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                invoice.IsPrinted = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkNoQuoteInvoicesAsPrintedAsync(List<int> invoiceIds)
        {
            // Retrieve the NoQuoteInvoices that need to be marked as printed
            var noQuoteInvoices = await _context.NoQuoteInvoices
                .Where(nqi => invoiceIds.Contains(nqi.Id))
                .ToListAsync();

            // Update the IsPrinted property
            foreach (var invoice in noQuoteInvoices)
            {
                invoice.IsPrinted = true;
            }

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

    }
}
