using PAS.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Invoice
{
    public int Id { get; set; } // Primary key

    // FK to InvoiceHeader
    public int InvoiceGroupId { get; set; }
    public InvoiceHeader InvoiceHeader { get; set; }

    // Line item fields
    public DateTime InvoiceDate { get; set; }

    [Required]
    public string InvoiceChemicalName { get; set; }

    [Required]
    public decimal InvoiceRatePerAcre { get; set; }

    [Required]
    public string InvoiceUnitOfMeasure { get; set; }
    public string UnitOfMeasure { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal InvoicePrice { get; set; }

    // Optional: computed line total
    [NotMapped]
    public decimal LineTotal => InvoiceRatePerAcre * InvoicePrice;

    public bool IsPrinted { get; set; }
    public bool IsGroupSelected { get; set; }

    public int? QuoteId { get; set; }
    public QuoteInventory QuoteInventory { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    [NotMapped]
    public List<string> AvailablePUOMs { get; set; }

    public int GroupId { get; set; } // LoadMixId

    public bool ChargeInterest { get; set; } = true;   // default: interest applies
    public decimal? InterestRate { get; set; }         // null = use customer default

    public int? QuoteInventoryId { get; set; }   // NEW FK
  
}
