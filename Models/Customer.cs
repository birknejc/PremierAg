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
        public string CustomerCell { get; set; }
        public string CustomerFax { get; set; }
        public string CustomerEmail { get; set; }
        public ICollection<Field> Fields { get; set; } = new List<Field>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    }
}
