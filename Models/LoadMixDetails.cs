﻿namespace PAS.Models
{
    public class LoadMixDetails
    {
        public int Id { get; set; }
        public int LoadMixId { get; set; } // Link back to the LoadMix
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
