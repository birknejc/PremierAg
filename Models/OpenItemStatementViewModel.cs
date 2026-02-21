namespace PAS.Models
{
    public class OpenItemStatementViewModel
    {
        public int CustomerId { get; set; } 
        public string CustomerName { get; set; } 
        public DateTime StatementDate { get; set; } 
        public decimal TotalBalance { get; set; } 
        public decimal PrepaymentBalance { get; set; } 
        public decimal CurrentTotal { get; set; } 
        public decimal Days30Total { get; set; } 
        public decimal Days60Total { get; set; } 
        public decimal Days90Total { get; set; } 
        public decimal Days120PlusTotal { get; set; } 
        public List<StatementLine> Lines { get; set; } = new();
        public List<InvoiceInterestResult> InterestDetails { get; set; } = new();

        // ⭐ Total interest across all invoices
        public decimal TotalInterestDue { get; set; }

        // ⭐ Interest grouped by invoice group
        public Dictionary<int, decimal> InterestByGroup { get; set; } = new();

        // ⭐ Combined total (principal + interest)
        public decimal GrandTotalWithInterest { get; set; }

    }
}
