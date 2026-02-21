namespace PAS.Models
{
    public class FullStatementLine
    {
        public DateTime Date { get; set; }
        public string DocumentType { get; set; } = ""; // "Invoice", "Payment", "Prepayment", "Interest"
        public string Reference { get; set; } = ""; // Invoice #, Check #, etc.
        public string Description { get; set; } = "";
        public decimal InvoiceAmount { get; set; } // + for invoices
        public decimal PaymentAmount { get; set; } // - for payments
        public decimal PrepaymentAmount { get; set; } // - when recorded, + when applied
        public decimal InterestAmount { get; set; } // + interest
        public decimal RunningBalance { get; set; } // computed in order by date }
    }
}