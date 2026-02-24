namespace PAS.Models
{
    public enum LicenseOwnerType
    {
        Customer = 1,
        Employee = 2,
        Business = 3
    }

    public class ApplicatorLicense
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? CertNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string LicenseType { get; set; }
        public bool IsActive { get; set; }
        public bool PrintOnInvoice { get; set; }

        public LicenseOwnerType? OwnerType { get; set; }

        // ⭐ NEW: Link to Customer
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // ⭐ NEW: Link to Employee (optional)

    }


}
