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
    public class InvoiceService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private AppDbContext _context;
        private readonly IJSRuntime _jsRuntime;
        private readonly IWebHostEnvironment _env;
        private int _nextInvoiceGroupId = 0;


        public InvoiceService(IDbContextFactory<AppDbContext> contextFactory, IJSRuntime jsRuntime, IWebHostEnvironment env)
        {
            _contextFactory = contextFactory;
            _jsRuntime = jsRuntime;
            _env = env;
            // Create the initial DbContext instance
            _context = _contextFactory.CreateDbContext();
        }
        private void ResetContext() { _context.Dispose(); _context = _contextFactory.CreateDbContext(); }


        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.QuoteInventory)
                .Include(i => i.InvoiceHeader)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                await _jsRuntime.InvokeVoidAsync("console.log", $"🔍 Loaded Invoice ID: {invoice.Id}, UOM: {invoice.InvoiceUnitOfMeasure}, Price: {invoice.InvoicePrice}");
            }

            foreach (var invoice in invoices)
            {
                if (invoice.InvoicePrice == 0 && (string.IsNullOrWhiteSpace(invoice.InvoiceUnitOfMeasure)))
                {
                    var quom = ExtractAlpha(invoice.InvoiceRatePerAcre.ToString())?.ToLower().Trim();

                    var puoms = await _context.UOMConversions
                        .Where(c => c.QUOM.ToLower().Trim() == quom)
                        .Select(c => c.PUOM)
                        .Distinct()
                        .ToListAsync();

                    invoice.AvailablePUOMs = puoms;
                }
            }

            return invoices;
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.QuoteInventory)
                .Include(i => i.InvoiceHeader)
                .ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.QuoteInventory)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Invoice>> GenerateInvoicesAsync()
        {
            ResetContext();

            var invoicesWithQuote = await GenerateInvoicesWithQuoteIdAsync() ?? new List<Invoice>();
            return invoicesWithQuote;
        }


        private async Task<List<Invoice>> GenerateInvoicesWithQuoteIdAsync()
        {
            var invoicesToSave = new List<Invoice>();
            var existingInvoiceKeys = new HashSet<string>();

            var quotes = await _context.QuoteInventories
                .Include(qi => qi.Quote)
                .Include(qi => qi.Quote.Customer)
                .ToListAsync();

            var loadMixes = await _context.LoadMixes
                .Include(lm => lm.LoadMixDetails)
                .Include(lm => lm.LoadFields)
                .ToListAsync();

            // Collect invoices per customer BEFORE creating headers
            var customerInvoicesMap = new Dictionary<int, List<Invoice>>();

            foreach (var loadMix in loadMixes)
            {
                var loadFieldIds = loadMix.LoadFields
                    .Select(lf => lf.SelectedFieldId)
                    .Distinct()
                    .ToList();

                var customerFieldSplits = await _context.CustomerFields
                    .Where(cf => loadFieldIds.Contains(cf.FieldId))
                    .ToListAsync();

                var allCustomerIds = new HashSet<int>();

                foreach (var lf in loadMix.LoadFields)
                {
                    allCustomerIds.Add(lf.CustomerId);

                    foreach (var split in customerFieldSplits.Where(cf => cf.FieldId == lf.SelectedFieldId))
                        allCustomerIds.Add(split.CustomerId);
                }

                foreach (var customerId in allCustomerIds)
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
                    if (customer == null)
                        continue;

                    // ⭐ Prevent duplicate generation for this customer
                    bool customerAlreadyHasInvoicesForLoadMix = await _context.Invoices
                        .AnyAsync(i =>
                            i.CustomerId == customerId &&
                            i.GroupId == loadMix.LoadMixId); // GroupId is your LoadMixId
                                                             //
                    if (customerAlreadyHasInvoicesForLoadMix) continue;

                    var customerFields = loadMix.LoadFields
                        .Where(lf =>
                            lf.CustomerId == customerId ||
                            customerFieldSplits.Any(cf => cf.FieldId == lf.SelectedFieldId && cf.CustomerId == customerId))
                        .ToList();

                    // ⭐ Diagnostic logging — this is what you asked for
                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $"[FieldsForCustomer] LoadMix {loadMix.LoadMixId}, Customer {customerId} has {customerFields.Count} fields: " +
                        $"{string.Join(", ", customerFields.Select(f => f.SelectedFieldId))}");

                    if (!customerFields.Any())
                        continue;

                    var invoicesForCustomer = customerInvoicesMap.GetValueOrDefault(customerId) ?? new List<Invoice>();

                    foreach (var detail in loadMix.LoadMixDetails)
                    {
                        if (string.IsNullOrWhiteSpace(detail.Product) ||
                            detail.Product.Equals("Water", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Determine if this customer has normal fields or customer-applied rows
                        bool hasCustomerAppliedRow =
                            customerFields.Any(f => f.SelectedFieldId == 0 &&
                                                    f.FieldName == "Applied by customer");

                        bool hasNormalFieldRow =
                            customerFields.Any(f => f.SelectedFieldId != 0);

                        // Skip Application ONLY if the customer has ONLY Customer Applied rows
                        if (detail.Product.Equals("Application", StringComparison.OrdinalIgnoreCase))
                        {
                            if (hasCustomerAppliedRow && !hasNormalFieldRow)
                                continue; // skip Application for this customer
                        }


                        // ⭐ Include LoadMixId to avoid false duplicates
                        var invoiceKey = $"{customerId}-{loadMix.LoadMixId}-{detail.Product.ToLower()}";
                        if (existingInvoiceKeys.Contains(invoiceKey))
                            continue;

                        decimal extractedRate = ExtractRatePerAcre(detail.RatePerAcre);

                        var matchingQuote = quotes.FirstOrDefault(q =>
                            q.ChemicalName != null &&
                            q.ChemicalName.ToLower() == detail.Product.ToLower() &&
                            q.Quote != null &&
                            q.Quote.CustomerId == customerId);

                        string invoiceUnitOfMeasure;
                        string unitOfMeasure;
                        List<string> availablePUOMs = null;

                        if (matchingQuote != null)
                        {
                            invoiceUnitOfMeasure = matchingQuote.UnitOfMeasure;
                            unitOfMeasure = matchingQuote.UnitOfMeasure;
                        }
                        else
                        {
                            var quom = ExtractAlpha(detail.RatePerAcre)?.ToLower().Trim();

                            availablePUOMs = await _context.UOMConversions
                                .Where(c => c.QUOM.ToLower().Trim() == quom)
                                .Select(c => c.PUOM)
                                .Distinct()
                                .ToListAsync();

                            invoiceUnitOfMeasure = "pending";
                            unitOfMeasure = "pending";
                        }

                        var invoice = new Invoice
                        {
                            GroupId = loadMix.LoadMixId,
                            InvoiceChemicalName = detail.Product,
                            InvoiceUnitOfMeasure = invoiceUnitOfMeasure,
                            UnitOfMeasure = unitOfMeasure,
                            InvoicePrice = matchingQuote != null ? matchingQuote.QuotePrice : 0,
                            //QuoteId = matchingQuote?.QuoteId,
                            QuoteId = matchingQuote?.Quote?.Id,
                            CustomerId = customerId,
                            InvoiceRatePerAcre = 0,
                            IsPrinted = false,
                            AvailablePUOMs = availablePUOMs,
                            InvoiceDate = DateTime.UtcNow
                        };
                        await _jsRuntime.InvokeVoidAsync("console.log", $"[InvoiceGen] Product={detail.Product}, Customer={customerId}, " + $"matchingQuote={(matchingQuote != null ? "YES" : "NO")}, " + $"QuoteIdAssigned={invoice.QuoteId}");

                        // ⭐ RatePerAcre calculation
                        if (invoice.UnitOfMeasure != "pending" && invoice.InvoiceUnitOfMeasure != "pending")
                        {
                            foreach (var lf in customerFields)
                            {
                                if (lf.FieldAcres == 0 || extractedRate == 0)
                                    continue;

                                var suom = invoice.UnitOfMeasure?.ToLower().Trim();
                                var puom = invoice.InvoiceUnitOfMeasure?.ToLower().Trim();

                                var conversion = await _context.UOMConversions.FirstOrDefaultAsync(uom =>
                                    uom.SUOM.ToLower().Trim() == suom &&
                                    uom.PUOM.ToLower().Trim() == puom);

                                decimal conversionFactor = conversion?.ConversionFactor ?? 1.0M;

                                decimal numerator = lf.FieldAverageRate * lf.FieldAcres * extractedRate;
                                decimal denominator = loadMix.LMRatePerAcre * conversionFactor;
                                decimal newRatePerAcre = denominator != 0 ? numerator / denominator : 0;

                                invoice.InvoiceRatePerAcre += newRatePerAcre;
                            }

                            invoice.InvoiceRatePerAcre = Math.Round(invoice.InvoiceRatePerAcre, 2);
                        }
                        else
                        {
                            if (matchingQuote == null)
                                invoice.InvoiceRatePerAcre = extractedRate;
                        }

                        invoicesForCustomer.Add(invoice);
                        existingInvoiceKeys.Add(invoiceKey);
                    }


                    if (invoicesForCustomer.Any())
                        customerInvoicesMap[customerId] = invoicesForCustomer;
                }
            }

            // ⭐ Compute nextGroupId ONCE — no DB calls inside loop
            //int nextGroupId = (await _context.InvoiceHeaders
            //    .MaxAsync(h => (int?)h.InvoiceGroupId) ?? 999) + 1;
            int? maxGroupId = await _context.InvoiceHeaders
                .Select(h => (int?)h.InvoiceGroupId)
                .MaxAsync();

            int nextGroupId = (maxGroupId ?? 999) + 1;


            // ⭐ Create headers and assign group IDs
            foreach (var kvp in customerInvoicesMap)
            {
                int customerId = kvp.Key;
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
                if (customer == null)
                    continue;

                var header = new InvoiceHeader
                {
                    InvoiceGroupId = nextGroupId,
                    CustomerId = customerId,
                    CreatedDate = DateTime.UtcNow,
                    TotalAmount = 0,
                    AmountPaid = 0,
                    Status = "Draft",
                    DueDate = DateTime.UtcNow.AddDays(customer.PaymentTermsDays)
                };

                _context.InvoiceHeaders.Add(header);

                foreach (var invoice in kvp.Value)
                {
                    invoice.InvoiceGroupId = nextGroupId;
                    header.TotalAmount += invoice.InvoiceRatePerAcre * invoice.InvoicePrice;
                    _context.Invoices.Add(invoice);
                    invoicesToSave.Add(invoice);
                }

                nextGroupId++; // ⭐ increment locally
            }

            if (invoicesToSave.Any())
                await _context.SaveChangesAsync();

            return invoicesToSave;
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

        private string ExtractAlpha(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Extract all alphabetic characters (considering multi-word units like "fl oz")
            var result = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetter(c) || c == ' ') // Include letters and spaces
                {
                    result.Append(c);
                }
            }

            // Trim any extra spaces and return the result
            return result.ToString().Trim();
        }
        public async Task AddInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            var existingInvoice = await _context.Invoices.FindAsync(invoice.Id);
            if (existingInvoice != null)
            {
                existingInvoice.InvoicePrice = invoice.InvoicePrice;
                existingInvoice.InvoiceRatePerAcre = invoice.InvoiceRatePerAcre;
                existingInvoice.InvoiceUnitOfMeasure = invoice.InvoiceUnitOfMeasure;
                existingInvoice.UnitOfMeasure = invoice.UnitOfMeasure;
                existingInvoice.ChargeInterest = invoice.ChargeInterest;
                existingInvoice.InterestRate = invoice.InterestRate;

                await _context.SaveChangesAsync();

                // ⭐ Recalculate the header total AFTER invoice changes
                await RecalculateInvoiceHeaderTotalAsync(existingInvoice.InvoiceGroupId);
            }
        }

        public async Task DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return;

            int groupId = invoice.InvoiceGroupId;

            // Delete the invoice
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            // Check if any invoices remain in this group
            bool anyRemaining = await _context.Invoices
                .AnyAsync(i => i.InvoiceGroupId == groupId);

            if (!anyRemaining)
            {
                var header = await _context.InvoiceHeaders
                    .FirstOrDefaultAsync(h => h.InvoiceGroupId == groupId);

                if (header != null)
                {
                    _context.InvoiceHeaders.Remove(header);
                    await _context.SaveChangesAsync();
                }
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

        public async Task UpdateInvoiceUnitAsync(Invoice invoice, string selectedPUOM)
        {
            await _jsRuntime.InvokeVoidAsync("console.log", $"🔄 Starting UpdateInvoiceUnitAsync for Invoice ID: {invoice.Id}");
            await _jsRuntime.InvokeVoidAsync("console.log", $"🟡 Selected PUOM: {selectedPUOM}");

            if (string.IsNullOrWhiteSpace(selectedPUOM))
                return;

            // 1. Resolve conversion
            var conversion = await _context.UOMConversions
                .FirstOrDefaultAsync(c => c.PUOM.ToLower().Trim() == selectedPUOM.ToLower().Trim());

            string resolvedSUOM = conversion?.SUOM ?? "unknown";
            decimal conversionFactor = conversion?.ConversionFactor ?? 1.0M;

            await _jsRuntime.InvokeVoidAsync("console.log",
                conversion != null
                    ? $"✅ Found conversion: PUOM='{conversion.PUOM}', SUOM='{conversion.SUOM}', Factor={conversionFactor}"
                    : $"⚠️ No conversion found for PUOM='{selectedPUOM}'. Using SUOM='unknown', Factor=1.0");

            invoice.InvoiceUnitOfMeasure = selectedPUOM;
            invoice.UnitOfMeasure = resolvedSUOM;

            // 2. Load LoadMix (for LMRatePerAcre + details)
            var loadMix = await _context.LoadMixes
                .Include(lm => lm.LoadMixDetails)
                .FirstOrDefaultAsync(lm => lm.LoadMixId == invoice.GroupId);

            if (loadMix == null)
            {
                await _jsRuntime.InvokeVoidAsync("console.log", "❌ LoadMix not found.");
                return;
            }

            // 3. Load the LoadMixDetail for THIS chemical
            var detail = loadMix.LoadMixDetails
                .FirstOrDefault(d => d.Product.ToLower() == invoice.InvoiceChemicalName.ToLower());

            if (detail == null)
            {
                await _jsRuntime.InvokeVoidAsync("console.log", "❌ LoadMixDetail not found for this chemical.");
                return;
            }

            decimal extractedRate = ExtractRatePerAcre(detail.RatePerAcre);
            await _jsRuntime.InvokeVoidAsync("console.log", $"📌 ExtractedRate from LoadMixDetail: {extractedRate}");

            // 4. Load LoadFields using the CORRECT KEY: GroupId == InvoiceGroupId
            var loadFields = await _context.LoadFields
                //.Where(lf => lf.GroupId == invoice.InvoiceGroupId)
                .Where(lf => lf.GroupId == invoice.GroupId)
                .ToListAsync();

            await _jsRuntime.InvokeVoidAsync("console.log",
                $"📊 Loaded {loadFields.Count} LoadField row(s) for GroupId={invoice.InvoiceGroupId}, CustomerId={invoice.CustomerId}");

            // 5. Load split info for those fields
            var fieldIds = loadFields.Select(lf => lf.SelectedFieldId).Distinct().ToList();

            var customerSplits = await _context.CustomerFields
                .Where(cf => fieldIds.Contains(cf.FieldId))
                .ToListAsync();

            await _jsRuntime.InvokeVoidAsync("console.log",
                $"📊 Loaded {customerSplits.Count} CustomerField split row(s) for these fields.");

            // 6. Calculate RatePerAcre
            decimal totalRate = 0;

            foreach (var lf in loadFields)
            {
                bool isDirect = lf.CustomerId == invoice.CustomerId;

                var split = customerSplits.FirstOrDefault(cf =>
                    cf.FieldId == lf.SelectedFieldId &&
                    cf.CustomerId == invoice.CustomerId);

                if (!isDirect && split == null)
                    continue;

                decimal splitFactor = isDirect ? 1.0M : (split.InvoiceSplit / 100.0M);

                decimal acres = lf.FieldAcres * splitFactor;
                decimal avgRate = lf.FieldAverageRate;

                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"🌱 Field {lf.SelectedFieldId}: Direct={isDirect}, SplitFactor={splitFactor}, Acres={acres}, AvgRate={avgRate}");

                if (acres == 0 || extractedRate == 0)
                    continue;

                decimal numerator = avgRate * acres * extractedRate;
                decimal denominator = loadMix.LMRatePerAcre * conversionFactor;
                decimal rate = denominator != 0 ? numerator / denominator : 0;

                totalRate += rate;

                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"➕ Contribution: numerator={numerator}, denominator={denominator}, rate={rate}");
            }

            invoice.InvoiceRatePerAcre = Math.Round(totalRate, 2);
            await _jsRuntime.InvokeVoidAsync("console.log",
                $"📐 FINAL InvoiceRatePerAcre: {invoice.InvoiceRatePerAcre}");

            // 7. Save to DB
            var existingInvoice = await _context.Invoices.FindAsync(invoice.Id);
            if (existingInvoice != null)
            {
                existingInvoice.InvoiceUnitOfMeasure = invoice.InvoiceUnitOfMeasure;
                existingInvoice.UnitOfMeasure = invoice.UnitOfMeasure;
                existingInvoice.InvoiceRatePerAcre = invoice.InvoiceRatePerAcre;
                existingInvoice.InvoicePrice = invoice.InvoicePrice;

                await _context.SaveChangesAsync();
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"✅ Saved to DB: ID={invoice.Id}, PUOM={invoice.InvoiceUnitOfMeasure}, SUOM={invoice.UnitOfMeasure}, RatePerAcre={invoice.InvoiceRatePerAcre}");

                await RecalculateInvoiceHeaderTotalAsync(existingInvoice.InvoiceGroupId);
            }
        }

        public async Task<int> GenerateNewInvoiceGroupId()
        {
            var maxGroupId = await _context.InvoiceHeaders
                .MaxAsync(h => (int?)h.InvoiceGroupId) ?? 999;

            return maxGroupId + 1;
        }


        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.OrderBy(c => c.CustomerBusinessName).ToListAsync();
        }

        public async Task RecalculateInvoiceHeaderTotalAsync(int invoiceGroupId)
        {
            var header = await _context.InvoiceHeaders
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == invoiceGroupId);

            if (header == null)
                return;

            var lines = await _context.Invoices
                .Where(i => i.InvoiceGroupId == invoiceGroupId)
                .ToListAsync();

            decimal sum = 0;

            foreach (var line in lines)
            {
                var lineAmount = line.InvoiceRatePerAcre * line.InvoicePrice;
                sum += lineAmount;

                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"[Recalc] Group {invoiceGroupId} Line Id={line.Id} Rate={line.InvoiceRatePerAcre} Price={line.InvoicePrice} LineAmount={lineAmount}");
            }

            await _jsRuntime.InvokeVoidAsync("console.log",
                $"[Recalc] Group {invoiceGroupId} FINAL Total={sum}");

            header.TotalAmount = sum;

            await _context.SaveChangesAsync();
        }

        public async Task ApplyPaymentAsync(int invoiceGroupId, decimal amount, string method)
        {
            var header = await _context.InvoiceHeaders
                .Include(h => h.Payments)
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == invoiceGroupId);

            if (header == null)
                throw new Exception("Invoice not found.");

            var payment = new Payment
            {
                CustomerId = header.CustomerId,
                InvoiceGroupId = invoiceGroupId,
                Amount = amount,
                Method = method,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await UpdateInvoicePaymentStatusAsync(header);
        }

        public async Task ApplyPrepaymentAsync(int customerId, int invoiceGroupId, decimal amount)
        {
            var header = await _context.InvoiceHeaders
                .Include(h => h.Payments)
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == invoiceGroupId);

            if (header == null)
                throw new Exception("Invoice not found.");

            var payment = new Payment
            {
                CustomerId = customerId,
                InvoiceGroupId = invoiceGroupId,
                Amount = amount,
                Method = "Prepayment",
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await UpdateInvoicePaymentStatusAsync(header);
        }

        public async Task AddPrepaymentAsync(int customerId, decimal amount, string method, string note)
        {
            var payment = new Payment
            {
                CustomerId = customerId,
                InvoiceGroupId = null, // prepayment
                Amount = amount,
                Method = method,
                PaymentDate = DateTime.UtcNow,
                Note = note
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateInvoicePaymentStatusAsync(InvoiceHeader header)
        {
            // Recompute from Payments table
            var amountPaid = header.Payments.Sum(p => p.Amount);

            header.AmountPaid = amountPaid;   // ⭐ REQUIRED ⭐

            if (amountPaid >= header.TotalAmount)
            {
                header.Status = "Paid";
                header.PaidDate = DateTime.UtcNow;
            }
            else if (amountPaid > 0)
            {
                header.Status = "PartiallyPaid";
                header.PaidDate = null;
            }
            else
            {
                header.Status = "Printed";
                header.PaidDate = null;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<decimal> GetCustomerPrepaymentBalanceAsync(int customerId)
        {
            return await _context.Payments
                .Where(p => p.CustomerId == customerId && p.InvoiceGroupId == null)
                .SumAsync(p => p.Amount);
        }


        public async Task<OpenItemStatementViewModel> GetOpenItemStatementAsync(int customerId, DateTime asOfDate, List<InvoiceInterestResult> interestResults)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                return null;

            var printedGroupIds = await _context.Invoices
                .Where(i => i.CustomerId == customerId && i.IsPrinted)
                .Select(i => i.InvoiceGroupId)
                .Distinct()
                .ToListAsync();

            var headers = await _context.InvoiceHeaders
                .Include(h => h.Payments)
                .Where(h => printedGroupIds.Contains(h.InvoiceGroupId))
                .ToListAsync();

            var lines = new List<StatementLine>();

            foreach (var h in headers)
            {
                var amountPaid = h.Payments?.Sum(p => p.Amount) ?? 0m;
                var balanceDue = h.TotalAmount - amountPaid;

                if (balanceDue <= 0)
                    continue;

                var invoiceDate = await _context.Invoices
                    .Where(i => i.InvoiceGroupId == h.InvoiceGroupId)
                    .Select(i => i.InvoiceDate)
                    .FirstOrDefaultAsync();

                var daysPastDue = (asOfDate.Date - h.DueDate.Date).Days;

                var line = new StatementLine
                {
                    InvoiceDate = invoiceDate,
                    InvoiceGroupId = h.InvoiceGroupId,
                    TotalAmount = h.TotalAmount,
                    AmountPaid = amountPaid,
                    BalanceDue = balanceDue
                };

                if (daysPastDue <= 0)
                    line.Current = balanceDue;
                else if (daysPastDue <= 30)
                    line.Days30 = balanceDue;
                else if (daysPastDue <= 60)
                    line.Days60 = balanceDue;
                else if (daysPastDue <= 90)
                    line.Days90 = balanceDue;
                else
                    line.Days120Plus = balanceDue;

                lines.Add(line);
            }

            var prepaymentBalance = await GetCustomerPrepaymentBalanceAsync(customerId);

            // Use the interest results passed in from the page
            decimal totalInterest = interestResults.Sum(i => i.InterestAmount);

            var interestByGroup = interestResults
                .GroupBy(i => i.InvoiceGroupId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.InterestAmount));

            var vm = new OpenItemStatementViewModel
            {
                CustomerId = customerId,
                CustomerName = customer.CustomerBusinessName,
                StatementDate = asOfDate,
                PrepaymentBalance = prepaymentBalance,
                Lines = lines,

                InterestDetails = interestResults,
                TotalInterestDue = totalInterest,
                InterestByGroup = interestByGroup,

                TotalBalance = lines.Sum(l => l.BalanceDue),
                CurrentTotal = lines.Sum(l => l.Current),
                Days30Total = lines.Sum(l => l.Days30),
                Days60Total = lines.Sum(l => l.Days60),
                Days90Total = lines.Sum(l => l.Days90),
                Days120PlusTotal = lines.Sum(l => l.Days120Plus)
            };

            vm.GrandTotalWithInterest = vm.TotalBalance + vm.TotalInterestDue;

            return vm;
        }

        public async Task<string> BuildSingleInvoiceHtmlAsync(List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
                return "<h3>No invoice data available.</h3>";

            var first = invoices.First();
            int groupId = first.InvoiceGroupId;

            var header = await _context.InvoiceHeaders
                .Include(h => h.Customer)
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == groupId);

            if (header == null)
                return "<h3>Invoice header not found.</h3>";

            // Load template
            var templatePath = Path.Combine(_env.WebRootPath, "print_invoicetemplate.html");
            var html = await File.ReadAllTextAsync(templatePath);

            // ---------------------------------------------------------
            // LOAD APPLICATOR LICENSES (dynamic, grouped, printable)
            // ---------------------------------------------------------
            var applicators = await _context.ApplicatorLicenses
                .Where(a => a.IsActive && a.PrintOnInvoice)
                .OrderBy(a => a.LicenseType)   // Certification first
                .ThenBy(a => a.Name)
                .ToListAsync();

            var applicatorSb = new StringBuilder();
            applicatorSb.AppendLine("<p><strong>Commercial Pesticide Applicator</strong></p>");

            // Certifications first
            foreach (var a in applicators.Where(a => a.LicenseType == "Certification"))
            {
                applicatorSb.AppendLine(
                    $"<p>{a.Name} Cert # {a.CertNumber} (Expires {a.ExpirationDate:yyyy-MM-dd})</p>");
            }

            // Business license rows second
            foreach (var a in applicators.Where(a => a.LicenseType == "BusinessLicense"))
            {
                applicatorSb.AppendLine(
                    $"<p>Pesticide Applicator License #{a.LicenseNumber} (Expires {a.ExpirationDate:yyyy-MM-dd})</p>");
            }

            // ---------------------------------------------------------
            // BUILD INVOICE HTML
            // ---------------------------------------------------------
            var sb = new StringBuilder();

            sb.AppendLine($@"
    <!-- TOP ROW: LOGO LEFT, INVOICE META RIGHT -->
    <div class='header-row'>
        <img class='logo' src='/images/logo.jpg' alt='Company Logo' />

        <div style='text-align:right;'>
            <h2 style='margin:0;'>INVOICE</h2>
            <div style='height:10px;'></div>
            <p><strong>Date:</strong> {header.CreatedDate.ToShortDateString()}</p>
            <p><strong>Invoice #:</strong> {header.InvoiceGroupId}</p>
        </div>
    </div>

    <!-- SECOND ROW: BILL TO LEFT, APPLICATOR BLOCK RIGHT -->
    <div class='content-row'>
        <div class='content-column1 bill-to'>
            <p><strong>Bill To:</strong></p>
            <p>{header.Customer.CustomerBusinessName}</p>
            <p>{header.Customer.CustomerStreet}</p>
            <p>{header.Customer.CustomerCity}, {header.Customer.CustomerState} {header.Customer.CustomerZipCode}</p>
        </div>

        <div class='content-column2 applicator-info'>
            {applicatorSb.ToString()}
        </div>
    </div>

    <!-- TABLE HEADER -->
    <table>
        <thead>
            <tr>
                <th>Qty</th>
                <th>Unit</th>
                <th>Description</th>
                <th>Price/Unit</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
");

            // ---------------------------------------------------------
            // INVOICE LINE ITEMS
            // ---------------------------------------------------------
            foreach (var line in invoices)
            {
                decimal amount = line.InvoiceRatePerAcre * line.InvoicePrice;

                sb.AppendLine($@"
            <tr>
                <td>{line.InvoiceRatePerAcre}</td>
                <td>{line.InvoiceUnitOfMeasure}</td>
                <td>{line.InvoiceChemicalName}</td>
                <td>{line.InvoicePrice.ToString("C")}</td>
                <td>{amount.ToString("C")}</td>
            </tr>
        ");
            }

            sb.AppendLine("</tbody></table>");

            // ---------------------------------------------------------
            // TOTALS
            // ---------------------------------------------------------
            sb.AppendLine($@"
        <div class='total-amount'>
            <p><strong>Total Amount:</strong> {header.TotalAmount.ToString("C")}</p>
            <p><strong>Amount Paid:</strong> {header.AmountPaid.ToString("C")}</p>
            <p><strong>Balance Due:</strong> {(header.TotalAmount - header.AmountPaid).ToString("C")}</p>
        </div>
    ");

            // Inject into template
            html = html.Replace("{{InvoicesContent}}", sb.ToString());

            html = html.Replace("{{InvoiceGroupId}}", header.InvoiceGroupId.ToString())
                       .Replace("{{InvoiceDate}}", header.CreatedDate.ToShortDateString())
                       .Replace("{{DueDate}}", header.DueDate.ToShortDateString())
                       .Replace("{{TotalAmount}}", header.TotalAmount.ToString("C"));

            return html;
        }



        public async Task<string> BuildInvoiceHtmlAsync(List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
                return string.Empty;

            var first = invoices.First();
            int groupId = first.InvoiceGroupId;

            var header = await _context.InvoiceHeaders
                .Include(h => h.Customer)
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == groupId);

            if (header == null)
                return string.Empty;

            var cust = header.Customer;

            // ---------------------------------------------------------
            // LOAD APPLICATOR LICENSES (dynamic, grouped, printable)
            // ---------------------------------------------------------
            var applicators = await _context.ApplicatorLicenses
                .Where(a => a.IsActive && a.PrintOnInvoice)
                .OrderBy(a => a.LicenseType)   // Certification first
                .ThenBy(a => a.Name)
                .ToListAsync();

            var applicatorSb = new StringBuilder();
            applicatorSb.AppendLine("<p><strong>Commercial Pesticide Applicator</strong></p>");

            // Certifications first
            foreach (var a in applicators.Where(a => a.LicenseType == "Certification"))
            {
                applicatorSb.AppendLine(
                    $"<p>{a.Name} Cert # {a.CertNumber} (Expires {a.ExpirationDate:yyyy-MM-dd})</p>");
            }

            // Business license rows second
            foreach (var a in applicators.Where(a => a.LicenseType == "BusinessLicense"))
            {
                applicatorSb.AppendLine(
                    $"<p>Pesticide Applicator License #{a.LicenseNumber} (Expires {a.ExpirationDate:yyyy-MM-dd})</p>");
            }

            // ---------------------------------------------------------
            // BUILD INVOICE HTML
            // ---------------------------------------------------------
            var sb = new StringBuilder();

            sb.AppendLine($@"
<div class='invoice-block'>

    <!-- TOP ROW: LOGO LEFT, INVOICE META RIGHT -->
    <div class='header-row'>
        <img class='logo' src='/images/logo.jpg' alt='Company Logo' />

        <div style='text-align:right;'>
            <h2 style='margin:0;'>INVOICE</h2>
            <div style='height:10px;'></div>
            <p><strong>Date:</strong> {header.CreatedDate.ToShortDateString()}</p>
            <p><strong>Invoice #:</strong> {header.InvoiceGroupId}</p>
        </div>
    </div>

    <!-- SECOND ROW: BILL TO LEFT, APPLICATOR BLOCK RIGHT -->
    <div class='content-row'>
        <div class='content-column1 bill-to'>
            <p><strong>Bill To:</strong></p>
            <p>{cust.CustomerBusinessName}</p>
            <p>{cust.CustomerStreet}</p>
            <p>{cust.CustomerCity}, {cust.CustomerState} {cust.CustomerZipCode}</p>
            <p>{cust.CustomerPhone}</p>
        </div>

        <div class='content-column2 applicator-info'>
            {applicatorSb.ToString()}
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Qty</th>
                <th>Unit</th>
                <th>Description</th>
                <th>Price/Unit</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
");

            // ---------------------------------------------------------
            // INVOICE LINE ITEMS
            // ---------------------------------------------------------
            foreach (var line in invoices)
            {
                decimal amount = line.InvoiceRatePerAcre * line.InvoicePrice;

                sb.AppendLine($@"
            <tr>
                <td>{line.InvoiceRatePerAcre}</td>
                <td>{line.InvoiceUnitOfMeasure}</td>
                <td>{line.InvoiceChemicalName}</td>
                <td>{line.InvoicePrice.ToString("C")}</td>
                <td>{amount.ToString("C")}</td>
            </tr>
");
            }

            sb.AppendLine($@"
        </tbody>
    </table>

    <div class='total-amount'>
        <p><strong>Total Amount:</strong> {header.TotalAmount.ToString("C")}</p>
        <p><strong>Amount Paid:</strong> {header.AmountPaid.ToString("C")}</p>
        <p><strong>Balance Due:</strong> {(header.TotalAmount - header.AmountPaid).ToString("C")}</p>
    </div>

    <footer class='footer'>
        <h4>Remittance</h4>
        <table>
            <tr><td><strong>Invoice #:</strong></td><td>{header.InvoiceGroupId}</td></tr>
            <tr><td><strong>Date:</strong></td><td>{header.CreatedDate.ToShortDateString()}</td></tr>
            <tr><td><strong>Due Date:</strong></td><td>{header.DueDate.ToShortDateString()}</td></tr>
            <tr><td><strong>Amount Due:</strong></td><td>{header.TotalAmount.ToString("C")}</td></tr>
            <tr><td><strong>Amount Enclosed:</strong></td><td></td></tr>
        </table>
    </footer>

    <div class='page-break'></div>
</div>
");

            return sb.ToString();
        }


        public async Task<string> BuildMultiInvoiceHtmlAsync(IEnumerable<IGrouping<int, Invoice>> groups)
        {
            var templatePath = Path.Combine(_env.WebRootPath, "print_invoicetemplate_multi.html");
            var template = await File.ReadAllTextAsync(templatePath);

            var allContent = new StringBuilder();

            foreach (var group in groups)
            {
                var block = await BuildInvoiceHtmlAsync(group.ToList());
                allContent.Append(block);
            }

            return template.Replace("{{InvoicesContent}}", allContent.ToString());
        }




        public async Task<string> BuildStatementHtmlAsync(OpenItemStatementViewModel vm)
        {
            var templatePath = Path.Combine(_env.WebRootPath, "print_statementtemplate.html");
            var html = await File.ReadAllTextAsync(templatePath);

            html = html.Replace("{{CustomerName}}", vm.CustomerName)
                       .Replace("{{StatementDate}}", vm.StatementDate.ToShortDateString())
                       .Replace("{{PrepaymentBalance}}", vm.PrepaymentBalance.ToString("C"))
                       .Replace("{{TotalBalance}}", vm.TotalBalance.ToString("C"))
                       .Replace("{{CurrentTotal}}", vm.CurrentTotal.ToString("C"))
                       .Replace("{{Days30Total}}", vm.Days30Total.ToString("C"))
                       .Replace("{{Days60Total}}", vm.Days60Total.ToString("C"))
                       .Replace("{{Days90Total}}", vm.Days90Total.ToString("C"))
                       .Replace("{{Days120PlusTotal}}", vm.Days120PlusTotal.ToString("C"));

            var sb = new StringBuilder();

            foreach (var line in vm.Lines)
            {
                sb.AppendLine($@"
            <tr>
                <td>{line.InvoiceDate.ToShortDateString()}</td>
                <td>{line.InvoiceGroupId}</td>
                <td>{line.TotalAmount.ToString("C")}</td>
                <td>{line.AmountPaid.ToString("C")}</td>
                <td>{line.BalanceDue.ToString("C")}</td>
                <td>{line.Current.ToString("C")}</td>
                <td>{line.Days30.ToString("C")}</td>
                <td>{line.Days60.ToString("C")}</td>
                <td>{line.Days90.ToString("C")}</td>
                <td>{line.Days120Plus.ToString("C")}</td>
            </tr>");
            }

            html = html.Replace("{{Lines}}", sb.ToString());

            return html;
        }

        public async Task<List<OpenItemStatementViewModel>> GetAllOpenItemStatementsAsync(DateTime asOfDate)
        {
            var customers = await _context.Customers.ToListAsync();
            var results = new List<OpenItemStatementViewModel>();

            foreach (var customer in customers)
            {
                // Calculate interest for this customer using the same asOfDate
                var interestResults = await CalculateInterestForCustomerAsync(customer.Id, asOfDate);

                // Build the statement using the same date + interest results
                var vm = await GetOpenItemStatementAsync(
                    customer.Id,
                    asOfDate,
                    interestResults
                );

                if (vm != null && vm.Lines.Any())
                    results.Add(vm);
            }

            return results;
        }


        public async Task<List<Payment>> GetPaymentsForCustomerAsync(int customerId)
        {
            return await _context.Payments
                .Where(p => p.CustomerId == customerId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task ApplySpecificPrepaymentAsync(int prepaymentId, int invoiceGroupId, decimal amount)
        {
            // Load the original prepayment
            var prepayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == prepaymentId && p.InvoiceGroupId == null);

            if (prepayment == null)
                throw new Exception("Prepayment not found.");

            if (amount > prepayment.Amount)
                throw new Exception("Amount exceeds available prepayment balance.");

            // Reduce the original prepayment
            prepayment.Amount -= amount;

            // Create a new payment applied to the invoice
            var appliedPayment = new Payment
            {
                CustomerId = prepayment.CustomerId,
                InvoiceGroupId = invoiceGroupId,
                Amount = amount,
                Method = "Prepayment",
                PaymentDate = DateTime.UtcNow,
                Note = prepayment.Note // carry note forward for audit trail
            };

            _context.Payments.Add(appliedPayment);

            await _context.SaveChangesAsync();

            // Update invoice header status
            var header = await _context.InvoiceHeaders
                .Include(h => h.Payments)
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == invoiceGroupId);

            if (header != null)
                await UpdateInvoicePaymentStatusAsync(header);
        }

        public async Task<List<InvoiceInterestResult>> CalculateInterestForCustomerAsync(int customerId, DateTime asOfDate)
        {
            var results = new List<InvoiceInterestResult>();

            // Load unpaid invoices for this customer
            var invoices = await _context.Invoices
                .Include(i => i.InvoiceHeader)
                    .ThenInclude(h => h.Customer)
                .Where(i => i.CustomerId == customerId &&
                            i.InvoiceHeader.AmountPaid < i.InvoiceHeader.TotalAmount)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                // Log each invoice line before any filtering
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"[InterestCalc] START InvoiceId={invoice.Id}, GroupId={invoice.InvoiceGroupId}, " +
                    $"Chemical={invoice.InvoiceChemicalName}, ChargeInterest={invoice.ChargeInterest}, " +
                    $"LineRate={invoice.InterestRate}, CustomerDefaultRate={invoice.InvoiceHeader.Customer.DefaultInterestRate}");

                if (!invoice.ChargeInterest)
                {
                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $"[InterestCalc] SKIP InvoiceId={invoice.Id} because ChargeInterest=false");
                    continue;
                }

                // Determine interest rate (line-level overrides customer default)
                decimal rate = invoice.InterestRate ??
                               invoice.InvoiceHeader.Customer.DefaultInterestRate;

                // Daily past-due calculation
                DateTime due = invoice.InvoiceHeader.DueDate.Date;
                DateTime today = asOfDate.Date;

                int daysPastDue = (today - due).Days;

                // Log due date and days past due
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"[InterestCalc] InvoiceId={invoice.Id}, DueDate={due:MM/dd/yyyy}, " +
                    $"AsOfDate={today:MM/dd/yyyy}, DaysPastDue={daysPastDue}");

                if (daysPastDue <= 0)
                {
                    await _jsRuntime.InvokeVoidAsync("console.log",
                        $"[InterestCalc] SKIP InvoiceId={invoice.Id} because DaysPastDue <= 0");
                    continue;
                }

                // Principal for this line
                decimal principal = invoice.InvoiceRatePerAcre * invoice.InvoicePrice;

                // Log principal and rate
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"[InterestCalc] InvoiceId={invoice.Id}, Principal={principal}, RateUsed={rate}");

                // Daily simple interest: principal * rate * (days / 365)
                decimal interestAmount = principal * (rate / 100m) * (daysPastDue / 365m);

                // Log final interest amount
                await _jsRuntime.InvokeVoidAsync("console.log",
                    $"[InterestCalc] InvoiceId={invoice.Id}, InterestAmountRaw={interestAmount}, " +
                    $"InterestRounded={Math.Round(interestAmount, 2)}");

                results.Add(new InvoiceInterestResult
                {
                    InvoiceId = invoice.Id,
                    InvoiceGroupId = invoice.InvoiceGroupId,
                    Description = invoice.InvoiceChemicalName,
                    Principal = principal,
                    InterestRate = rate,
                    DaysPastDue = daysPastDue,
                    InterestAmount = Math.Round(interestAmount, 2)
                });
            }

            return results;
        }

        public async Task<List<InvoiceInterestResult>> CalculateInterestForCustomerAllHistoryAsync(int customerId, DateTime asOfDate)
        {
            var results = new List<InvoiceInterestResult>();

            var invoices = await _context.Invoices
                .Include(i => i.InvoiceHeader)
                    .ThenInclude(h => h.Customer)
                .Where(i => i.CustomerId == customerId)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                if (!invoice.ChargeInterest)
                    continue;

                var header = invoice.InvoiceHeader;
                if (header == null)
                    continue;

                decimal rate = invoice.InterestRate ?? header.Customer.DefaultInterestRate;

                DateTime due = header.DueDate.Date;
                DateTime endDate = (header.PaidDate?.Date ?? asOfDate.Date);

                int daysPastDue = (endDate - due).Days;
                if (daysPastDue <= 0)
                    continue;

                decimal principal = invoice.InvoiceRatePerAcre * invoice.InvoicePrice;
                decimal interestAmount = principal * (rate / 100m) * (daysPastDue / 365m);
                interestAmount = Math.Round(interestAmount, 2);

                results.Add(new InvoiceInterestResult
                {
                    InvoiceId = invoice.Id,
                    InvoiceGroupId = invoice.InvoiceGroupId,
                    Description = invoice.InvoiceChemicalName,
                    Principal = principal,
                    InterestRate = rate,
                    DaysPastDue = daysPastDue,
                    InterestAmount = interestAmount
                });
            }

            return results;
        }

        private int CalculateMonthsPastDue(InvoiceHeader header)
        {
            // Use the customer's payment terms
            int terms = header.Customer.PaymentTermsDays;

            if (terms <= 0)
                return 0;

            // Use CreatedDate as the invoice date
            var dueDate = header.CreatedDate.AddDays(terms);
            var now = DateTime.UtcNow;

            if (now <= dueDate)
                return 0;

            int months = ((now.Year - dueDate.Year) * 12) + now.Month - dueDate.Month;

            if (now.Day < dueDate.Day)
                months--;

            return Math.Max(months, 0);
        }

        public async Task<InvoiceHeader?> GetInvoiceHeaderAsync(int invoiceGroupId)
        {
            return await _context.InvoiceHeaders
                .Include(h => h.Payments)
                .FirstOrDefaultAsync(h => h.InvoiceGroupId == invoiceGroupId);
        }

        public async Task<FullCustomerStatementViewModel?> GetFullCustomerStatementAsync(int customerId, DateTime asOfDate)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                return null;

            var vm = new FullCustomerStatementViewModel
            {
                CustomerId = customerId,
                CustomerName = customer.CustomerBusinessName,
                StatementDate = asOfDate,
                StartingBalance = 0m
            };

            var events = new List<FullStatementLine>();

            // 1. All invoice headers for this customer
            var headers = await _context.InvoiceHeaders
                .Where(h => h.CustomerId == customerId)
                .ToListAsync();

            foreach (var h in headers)
            {
                events.Add(new FullStatementLine
                {
                    Date = h.CreatedDate,
                    DocumentType = "Invoice",
                    Reference = h.InvoiceGroupId.ToString(),
                    Description = $"Invoice #{h.InvoiceGroupId}",
                    InvoiceAmount = h.TotalAmount
                });
            }

            // 2. All payments (applied to invoices)
            var payments = await _context.Payments
                .Where(p => p.CustomerId == customerId && p.InvoiceGroupId != null)
                .ToListAsync();

            foreach (var p in payments)
            {
                bool isAppliedPrepayment = string.Equals(p.Method, "Prepayment", StringComparison.OrdinalIgnoreCase);

                if (isAppliedPrepayment)
                {
                    events.Add(new FullStatementLine
                    {
                        Date = p.PaymentDate,
                        DocumentType = "Prepayment Applied",
                        Reference = p.InvoiceGroupId?.ToString() ?? "",
                        Description = $"Prepayment applied to Invoice #{p.InvoiceGroupId}",
                        PrepaymentAmount = +p.Amount   // applied → increases balance (consumes credit)
                    });
                }
                else
                {
                    events.Add(new FullStatementLine
                    {
                        Date = p.PaymentDate,
                        DocumentType = "Payment",
                        Reference = p.InvoiceGroupId?.ToString() ?? "",
                        Description = $"Payment ({p.Method})",
                        PaymentAmount = p.Amount
                    });
                }
            }

            // 3. All prepayments recorded (no invoice yet)
            var prepayments = await _context.Payments
                .Where(p => p.CustomerId == customerId && p.InvoiceGroupId == null)
                .ToListAsync();

            foreach (var p in prepayments)
            {
                events.Add(new FullStatementLine
                {
                    Date = p.PaymentDate,
                    DocumentType = "Prepayment Recorded",
                    Reference = "",
                    Description = $"Prepayment ({p.Method})",
                    PrepaymentAmount = -p.Amount   // recorded → reduces balance (credit)
                });
            }

            // 4. All historical interest (Option 2)
            var interestResults = await CalculateInterestForCustomerAllHistoryAsync(customerId, asOfDate);

            foreach (var ir in interestResults)
            {
                var header = headers.FirstOrDefault(h => h.InvoiceGroupId == ir.InvoiceGroupId);
                var date = header?.DueDate ?? asOfDate;

                events.Add(new FullStatementLine
                {
                    Date = date,
                    DocumentType = "Interest",
                    Reference = ir.InvoiceGroupId.ToString(),
                    Description = $"Interest on Invoice #{ir.InvoiceGroupId} ({ir.Description})",
                    InterestAmount = ir.InterestAmount
                });
            }

            // 5. Sort and compute running balance
            events = events
                .OrderBy(e => e.Date)
                .ThenBy(e => e.DocumentType)
                .ThenBy(e => e.Reference)
                .ToList();

            decimal running = vm.StartingBalance;

            foreach (var e in events)
            {
                running += e.InvoiceAmount;
                running -= e.PaymentAmount;
                running += e.PrepaymentAmount;   // signed: negative when recorded, positive when applied
                running += e.InterestAmount;

                e.RunningBalance = running;
            }

            vm.Lines = events;
            vm.EndingBalance = running;

            vm.TotalInvoiced = events.Sum(e => e.InvoiceAmount);
            vm.TotalPayments = events.Sum(e => e.PaymentAmount);
            vm.TotalPrepayments = events.Sum(e => e.PrepaymentAmount);
            vm.TotalInterest = events.Sum(e => e.InterestAmount);

            return vm;
        }

        public async Task<int> CreateDirectSaleInvoiceAsync(DirectSaleDto dto)
        {
            // -------------------------------
            // 1. Validate Customer Exists
            // -------------------------------
            var customer = await _context.Customers.FindAsync(dto.CustomerId);
            if (customer == null)
                throw new Exception("Customer not found.");

            // -------------------------------
            // 2. Load Products for All Items
            // -------------------------------
            var productNames = dto.Items
                .Select(i => i.ChemicalName)
                .Distinct()
                .ToList();

            var productMap = await _context.Products
                .Where(p => productNames.Contains(p.Name))
                .ToDictionaryAsync(p => p.Name, p => p);

            // -------------------------------
            // 3. Check for Restricted Products
            // -------------------------------
            bool hasRestricted = dto.Items.Any(i =>
                productMap.TryGetValue(i.ChemicalName, out var p) &&
                p.Restricted == true);

            if (hasRestricted)
            {
                // Load customer licenses
                var licenses = await _context.ApplicatorLicenses
                    .Where(l =>
                        l.CustomerId == dto.CustomerId &&
                        l.OwnerType == LicenseOwnerType.Customer &&
                        l.IsActive &&
                        l.ExpirationDate >= DateTime.UtcNow)
                    .ToListAsync();

                if (!licenses.Any())
                {
                    throw new InvalidOperationException(
                        "This sale includes restricted products, but the customer has no valid, non-expired license.");
                }
            }

            // -------------------------------
            // 4. Create Invoice Header
            // -------------------------------
            var header = new InvoiceHeader
            {
                CustomerId = dto.CustomerId,
                CreatedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                TotalAmount = dto.Items.Sum(i => i.Quantity * i.Price),
                Status = "Printed"
            };

            _context.InvoiceHeaders.Add(header);
            await _context.SaveChangesAsync();

            // -------------------------------
            // 5. Create Invoice Lines
            // -------------------------------
            foreach (var item in dto.Items)
            {
                _context.Invoices.Add(new Invoice
                {
                    InvoiceGroupId = header.InvoiceGroupId,
                    InvoiceChemicalName = item.ChemicalName,
                    InvoiceRatePerAcre = item.Quantity,

                    // ⭐ REQUIRED: populate both fields
                    InvoiceUnitOfMeasure = item.UnitOfMeasure,
                    UnitOfMeasure = item.UnitOfMeasure,

                    InvoicePrice = item.Price,
                    CustomerId = dto.CustomerId,
                    InvoiceDate = header.CreatedDate
                });
            }

            await _context.SaveChangesAsync();

            return header.InvoiceGroupId;
        }


        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<ApplicatorLicense>> GetCustomerLicensesAsync(int customerId)
        {
            return await _context.ApplicatorLicenses
                .Where(l => l.CustomerId == customerId)
                .ToListAsync();
        }


    }
}
