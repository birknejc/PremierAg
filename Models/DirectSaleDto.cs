namespace PAS.Models
{
    public class DirectSaleDto
    {
        public int CustomerId { get; set; }
        public List<DirectSaleItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

}
