namespace PAS.Models
{
    //Not mapped to DB - used to return interest calculation results
    public class InvoiceInterestResult
    {
        public int InvoiceId { get; set; }
        public int InvoiceGroupId { get; set; }
        public string Description { get; set; } = "";
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int DaysPastDue { get; set; }
        public decimal InterestAmount { get; set; }
    }

}
