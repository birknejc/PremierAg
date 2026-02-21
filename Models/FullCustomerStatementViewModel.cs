namespace PAS.Models
{
    public class FullCustomerStatementViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = ""; 
        public DateTime StatementDate { get; set; }
        public decimal StartingBalance { get; set; } // usually 0, or prior carry-forward
        public decimal EndingBalance { get; set; } 
        public decimal TotalInvoiced { get; set; } 
        public decimal TotalPayments { get; set; } 
        public decimal TotalPrepayments { get; set; } 
        public decimal TotalInterest { get; set; } 
        public List<FullStatementLine> Lines { get; set; } = new(); }
    }
