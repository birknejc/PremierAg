namespace PAS.Models
{
    public class LoadMixDetails
    {
        public int Id { get; set; }
        public int LoadMixId { get; set; } // Foreign key referencing Id in LoadMixes
        public int GroupId { get; set; }   // New column, value from LoadMix.LoadMixId
        public string Product { get; set; } // ChemicalName from QuoteInventory
        public string RatePerAcre { get; set; } // Concatenation of QuantityPerAcre and UnitOfMeasure from QuoteInventory
        public string Total { get; set; } // Calculated field
        // Add nullable fields
        public string? EPA { get; set; }
        public decimal? Price { get; set; }
        public decimal? QuotePrice { get; set; }
        public string? QuoteUnitOfMeasure { get; set; }
        public LoadMix LoadMix { get; set; } // Navigation property
    }
}
