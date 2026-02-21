namespace PAS.Models
{
    public class ProductPurchase
    {
        public int Id { get; set; }

        // Link to the product identity
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Vendor who supplied this batch
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        // Quantities
        public decimal QuantityReceived { get; set; }
        public decimal QuantityRemaining { get; set; }

        // Cost
        public decimal PricePerUnit { get; set; }

        // Traceability
        public DateTime ReceivedDate { get; set; }
        public int? PurchaseOrderId { get; set; } // optional link to your existing PO
    }

}
