namespace PAS.Models
{
    public class InventoryAudit
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public decimal QuantityChanged { get; set; } // negative = consume, positive = restore

        public int GroupId { get; set; }
        public int LoadMixId { get; set; }
        public int LoadMixDetailsId { get; set; }

        public string Action { get; set; } // "Consume" or "Restore"
        public DateTime Timestamp { get; set; }
    }

}
