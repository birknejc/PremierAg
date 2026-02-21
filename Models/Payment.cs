using PAS.Models;

public class Payment
{
    public int Id { get; set; }

    // Who the payment belongs to
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    // If null → this is a prepayment
    // If not null → payment applied to a specific invoice group
    public int? InvoiceGroupId { get; set; }
    public InvoiceHeader InvoiceHeader { get; set; }

    // Amount of the payment
    public decimal Amount { get; set; }

    // When the payment was made
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    // Optional: Check #, ACH, Cash, etc.
    public string Method { get; set; }

    public string? Note { get; set; }

}
