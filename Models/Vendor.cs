namespace PAS.Models
{
    public class Vendor
    {
        public int Id { get; set; }
        public string BusinessName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string SalesRepName { get; set; }
        public string SalesRepPhone { get; set; }
        public string SalesRepEmail { get; set; }
        
        // Add the navigation property for Inventory
        public ICollection<Inventory> Inventories { get; set; }
    }

}
