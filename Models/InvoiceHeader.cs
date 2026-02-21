using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PAS.Models
{
    public class InvoiceHeader
    {
        [Key]
        public int InvoiceGroupId { get; set; }   // Primary Key (replaces your current grouping ID)

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }   // NEW: total of all line items

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }    // NEW: payments applied

        [NotMapped]
        public decimal BalanceDue => TotalAmount - AmountPaid;

        public string Status { get; set; } = "Draft";  // Draft, Printed, Paid, etc.

        // Navigation
        public List<Invoice> InvoiceLines { get; set; }
        public List<Payment> Payments { get; set; } = new();

        [NotMapped]
        public decimal CalculatedAmountPaid =>
            Payments?.Sum(p => p.Amount) ?? 0;

        [NotMapped]
        public decimal CalculatedBalanceDue =>
            TotalAmount - CalculatedAmountPaid;

        [NotMapped]
        public string CalculatedStatus
        {
            get
            {
                if (CalculatedAmountPaid >= TotalAmount)
                    return "Paid";

                if (DateTime.UtcNow > DueDate && CalculatedAmountPaid < TotalAmount)
                    return "Overdue";

                if (CalculatedAmountPaid > 0 && CalculatedAmountPaid < TotalAmount)
                    return "PartiallyPaid";

                return "Printed";
            }
        }

    }
}
