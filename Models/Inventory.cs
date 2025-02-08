namespace PAS.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public string ChemicalName { get; set; }
        public string EPA { get; set; }  // EPA number
        public string UnitOfMeasure { get; set; }
        public decimal Price { get; set; }
        public int VendorId { get; set; }  // Foreign Key to Vendor
        public Vendor Vendor { get; set; }  // Navigation Property (Optional, for lookup only)
        public ICollection<QuoteInventory> QuoteInventories { get; set; } = new List<QuoteInventory>();
    }
}
