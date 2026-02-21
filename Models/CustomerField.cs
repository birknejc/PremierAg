namespace PAS.Models
{
    //many-to-many relationship between Customer and Field
    public class CustomerField
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int FieldId { get; set; }
        public Field Field { get; set; }

        // Percentage of invoice assigned to this customer for this field
        public decimal InvoiceSplit { get; set; } // e.g., 50.0 for 50%
    }

}
