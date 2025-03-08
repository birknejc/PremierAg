using System.ComponentModel.DataAnnotations;

namespace PAS.Models
{
    public class QuoteInventory
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public int InventoryId { get; set; }
        public string ChemicalName { get; set; }
        public string EPA { get; set; }
        public decimal Price { get; set; }
        public decimal QuantityPerAcre { get; set; }
        public decimal QuotePrice { get; set; }
        public string QuoteUnitOfMeasure { get; set; }
        public string UnitOfMeasure { get; set; }

        // Concurrency token
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Inventory Inventory { get; set; }
        public Quote Quote { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
    }


}
