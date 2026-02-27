namespace PAS.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string PONumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }

        // NEW — required FK
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }

        // Snapshot for printing
        public string BusinessName { get; set; }

        public DateTime? PaymentDueDate { get; set; }
        public DateTime? DeliveryPickUpDate { get; set; }
        public string? PickUpLocation { get; set; }

        public List<PurchaseOrderItem> Items { get; set; } = new();
    }

}
