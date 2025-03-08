namespace PAS.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string PONumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; } // Nullable if not received yet
        public string BusinessName { get; set; } // Vendor's business name
        public DateTime PaymentDueDate { get; set; }
        public DateTime DeliveryPickUpDate { get; set; }
        public string PickUpLocation { get; set; }

        // Navigation property for related items
        public List<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}
