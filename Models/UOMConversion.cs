namespace PAS.Models
{
    public class UOMConversion
    {
        public int Id { get; set; }
        public string PUOM { get; set; } //Unit of Measure for purchase (Inventory.UnitOfMeasurePurchase)
        public decimal CFPurSold { get; set; } //divide the Inventory/Pruchase Price by this for invoicing
        public string SUOM { get; set; } //Unit of Measure for sold (Inventory.UnitOfMeasure)
        public string QUOM { get; set; } //Unit of Mesaure for quote (QuoteInventory.QuoteUnitOfMeasure)
        public decimal ConversionFactor { get; set; } //conversion factor
    }
}
