using PAS.Models;

public class LoadFields
{
    public int Id { get; set; }
    public int LoadMixId { get; set; }
    public int SelectedFieldId { get; set; } // New property to store the selected field ID
    public string FieldName { get; set; }
    public decimal FieldAverageRate { get; set; }
    public decimal FieldTotalGallons { get; set; }
    public decimal FieldAcres { get; set; }
    public LoadMix LoadMix { get; set; }
}

