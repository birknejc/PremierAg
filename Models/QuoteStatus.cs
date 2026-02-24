namespace PAS.Models
{
    public enum QuoteStatus
    {
        Active = 0,     // Quote is valid and can be used
        Archived = 1,   // Manually archived by user
        Expired = 2,    // Auto-archived when ArchiveDate passes
        Converted = 3   // (Future) Used to create a LoadMix
    }
}

