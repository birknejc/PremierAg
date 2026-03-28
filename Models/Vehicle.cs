namespace PAS.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }

        public int? YearPurchased { get; set; }
        public decimal? PurchasedAmount { get; set; }

        public bool Active { get; set; } = true;
    }

}
