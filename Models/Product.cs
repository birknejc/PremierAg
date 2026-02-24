using System.ComponentModel.DataAnnotations.Schema;

namespace PAS.Models
{
    public class Product
    {
        public int Id { get; set; }

        // Identity fields
        public string Name { get; set; }
        public string EPA { get; set; }
        public string DefaultUnitOfMeasure { get; set; }

        // Optional classification fields
        public string Category { get; set; }
        public string? Description { get; set; }

        public bool? Restricted { get; set; }

        // Navigation
        public ICollection<ProductPurchase> Purchases { get; set; } = new List<ProductPurchase>();
        //public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

        public decimal WeightedAveragePrice { get; set; }
        public decimal CurrentCost => WeightedAveragePrice;

        public decimal TotalQuantityAvailable => Purchases?.Sum(pp => pp.QuantityRemaining) ?? 0;

        public ICollection<ProductVendor> ProductVendors { get; set; }

        [NotMapped]
        public decimal HoldQuantity { get; set; }

    }

}
