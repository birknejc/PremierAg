using PAS.Models;

public class Inventory
{
    public int Id { get; set; }
    public string ChemicalName { get; set; }
    public string EPA { get; set; }
    public string UnitOfMeasurePurchase { get; set; }
    public decimal Price { get; set; } // Current price for existing inventory
    public string UnitOfMeasure { get; set; }
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public ICollection<QuoteInventory> QuoteInventories { get; set; } = new List<QuoteInventory>();

    // New properties
    public decimal QuantityOnHand { get; set; }
    public decimal WeightedAveragePrice { get; set; }
    public decimal QuantityQuoted { get; set; }

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
