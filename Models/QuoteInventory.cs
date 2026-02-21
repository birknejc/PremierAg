using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS.Models
{
    public class QuoteInventory
    {
        public int Id { get; set; }

        public int QuoteId { get; set; }

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

        public Quote Quote { get; set; }

        public ICollection<Invoice> Invoices { get; set; }

        [NotMapped]
        public bool Selected { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

        [NotMapped]
        public string ProductUOM { get; set; }

        [NotMapped]
        public string ProductEPA { get; set; }

        [NotMapped]
        public decimal? ProductWeightedAveragePrice { get; set; }


    }

}
