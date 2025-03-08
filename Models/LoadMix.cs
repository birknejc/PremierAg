namespace PAS.Models
{
    public class LoadMix
    {
        public int Id { get; set; }
        public int? QuoteId { get; set; } // Link back to the Quote
        public DateTime LoadDate { get; set; } // New field for date
        public TimeSpan LoadTime { get; set; } // New field for time
        public string Crop { get; set; } // New field for crop
        public int TotalGallons { get; set; } // New field for total gallons

        public int TotalAcres { get; set; }
        public int LMRatePerAcre { get; set; }

        public Quote Quote { get; set; }
        public List<LoadFields> LoadFields { get; set; } // Navigation property for LoadFields

        public List<LoadMixDetails> LoadMixDetails { get; set; } // Navigation property for LoadMixDetails
    }
}
