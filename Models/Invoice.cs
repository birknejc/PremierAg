using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS.Models
{
    public class Invoice
    {
        public int Id { get; set; } // Primary key

        [Required]
        public string InvoiceCustomer { get; set; } // Customer Business Name

        [Required]
        public decimal InvoiceRatePerAcre { get; set; } // Rate per Acre

        [Required]
        public string InvoiceUnitOfMeasure { get; set; } // Quote Unit of Measure
        public string UnitOfMeasure { get; set; }

        [Required]
        public string InvoiceChemicalName { get; set; } // Chemical Name

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal InvoicePrice { get; set; } // Price

        public bool IsPrinted { get; set; }
        public bool IsGroupSelected { get; set; } // New property for selecting groups of invoices

        // Foreign keys for QuoteInventory
        public int QuoteId { get; set; }
        public int InventoryId { get; set; }
        public QuoteInventory QuoteInventory { get; set; } // Navigation property

        // Foreign key for Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } // Navigation property
    }
}
