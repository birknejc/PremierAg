namespace PAS.Models
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }

        // Legacy
        //public int? InventoryId { get; set; }
        public string ChemicalName { get; set; }
        public string UnitOfMeasurePurchase { get; set; }
        public string EPANumber { get; set; }
        public decimal Price { get; set; }

        // New Product-based fields (not used yet)
        public int? ProductId { get; set; }
        public Product Product { get; set; }

        // Product snapshot fields
        public string ProductName { get; set; }
        public string ProductEPA { get; set; }
        public string ProductUOM { get; set; }
        public decimal ProductPurchasePrice { get; set; }

        public int QuantityOrdered { get; set; }
        public int QuantityReceived { get; set; }
        public int NewQuantityReceived { get; set; }
        public int QuantityUnreceived { get; set; }
        public bool FullyReceived => QuantityOrdered == QuantityReceived;
        public decimal TotalCost => Price * QuantityOrdered;
    }


}


