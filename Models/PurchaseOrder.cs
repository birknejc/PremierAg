namespace PAS.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string PONumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; } // Nullable if not received yet
        public string BusinessName { get; set; }
        public string ChemicalName { get; set; }
        public int InventoryId { get; set; }  // Foreign key to Inventory
        public Inventory Inventory { get; set; } // Navigation property to Inventory
        public string EPANumber { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Price { get; set; }
        public int QuantityOrdered { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public DateTime DeliveryPickUpDate { get; set; }
        public string PickUpLocation { get; set; }

    }
}
