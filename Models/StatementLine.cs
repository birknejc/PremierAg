namespace PAS.Models
{
    public class StatementLine { 
        public DateTime InvoiceDate { get; set; } 
        public int InvoiceGroupId { get; set; } 
        public decimal TotalAmount { get; set; } 
        public decimal AmountPaid { get; set; } 
        public decimal BalanceDue { get; set; } 
        public decimal Current { get; set; } 
        public decimal Days30 { get; set; } 
        public decimal Days60 { get; set; } 
        public decimal Days90 { get; set; } 
        public decimal Days120Plus { get; set; } }
}
