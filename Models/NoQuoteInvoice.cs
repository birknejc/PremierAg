using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS.Models
{
    public class NoQuoteInvoice
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string InvoiceCustomer { get; set; }

        [StringLength(255)]
        public string InvoiceChemicalName { get; set; }

        [StringLength(50)]
        public string InvoiceUnitOfMeasure { get; set; }

        [StringLength(50)]
        public string UnitOfMeasure { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InvoicePrice { get; set; }

        public int CustomerId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InvoiceRatePerAcre { get; set; }

        public bool IsPrinted { get; set; }
        public bool IsGroupSelected { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}
