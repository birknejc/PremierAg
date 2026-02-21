namespace PAS.Models
{
    public class ApplicatorLicense
    {
        public int Id { get; set; }

        // For certifications: "Eugene M Joos"
        // For business license: "Pesticide Applicator License"
        public string Name { get; set; }

        // Null for business license
        public string? CertNumber { get; set; }

        public string? LicenseNumber { get; set; }

        public DateTime ExpirationDate { get; set; }

        // "Certification" or "BusinessLicense"
        public string LicenseType { get; set; }

        public bool IsActive { get; set; }

        public bool PrintOnInvoice { get; set; }
    }

}
