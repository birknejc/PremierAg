using PAS.Models;
using System.ComponentModel.DataAnnotations;

namespace PAS.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public string CustomerBusinessName { get; set; }
        public string QuoteStreet { get; set; }
        public string QuoteCity { get; set; }
        public string QuoteState { get; set; }
        public string QuoteZipcode { get; set; }
        public string QuotePhone { get; set; }
        public DateTime QuoteDate { get; set; }
        public decimal QuoteTotal { get; set; }
        public decimal EstimatedAcres { get; set; }
        public QuoteStatus Status { get; set; } = QuoteStatus.Active;

        [Required]
        public DateTime ArchiveDate { get; set; }


        // New fields for multiple inventory items
        public ICollection<QuoteInventory> QuoteInventories { get; set; } = new List<QuoteInventory>();

        // Navigation Properties
        public Customer Customer { get; set; }

        //LoadMix
        public List<LoadMix> LoadMixes { get; set; } = new List<LoadMix>();
        //public List<LoadMix2> LoadMix2s { get; set; } = new();

    }

}