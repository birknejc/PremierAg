namespace PAS.Models
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
        public int InventoryId { get; set; }
        public string ChemicalName { get; set; }
        public string UnitOfMeasurePurchase { get; set; }
        public decimal Price { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityReceived { get; set; } // Track quantity received
        public int NewQuantityReceived { get; set; } // Add this property to handle the newly received quantity
        public int QuantityUnreceived { get; set; } // Add this property to handle the quantity to be unreceived
        public bool FullyReceived => QuantityOrdered == QuantityReceived; // Track if fully received
        public string EPANumber { get; set; }
        public decimal TotalCost => Price * QuantityOrdered;
    }
}


