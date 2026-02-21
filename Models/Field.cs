namespace PAS.Models
{
    public class Field
    {
        public int Id { get; set; }
        public string FieldName { get; set; }
        public double Acres { get; set; }
        public int CustomerId { get; set; } // Foreign key
        public Customer Customer { get; set; } // Navigation property
        public ICollection<CustomerField> CustomerFields { get; set; }
    }
}
