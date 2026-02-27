using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerBusinessName { get; set; }
        public string CustomerFName { get; set; }
        public string CustomerLName { get; set; }
        public string CustomerStreet { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZipCode { get; set; }
        public string CustomerPhone { get; set; }
        public string? CustomerCell { get; set; }
        public string? CustomerFax { get; set; }
        public string? CustomerEmail { get; set; }
        public int PaymentTermsDays { get; set; } = 30; // default Net 30
        public List<Payment> Payments { get; set; } = new();

        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<NoQuoteInvoice> NoQuoteInvoices { get; set; } = new List<NoQuoteInvoice>();
        public ICollection<CustomerField> CustomerFields { get; set; }

        [NotMapped]
        public decimal PrepaymentBalance =>
            (Payments?.Where(p => p.InvoiceGroupId == null).Sum(p => p.Amount) ?? 0)
            - (Payments?.Where(p => p.InvoiceGroupId != null).Sum(p => p.Amount) ?? 0);
        public decimal DefaultInterestRate { get; set; } = 0;   // percent per month

    }
}
