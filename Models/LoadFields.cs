using PAS.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class LoadFields
{
    private int _selectedFieldId;

    public int Id { get; set; }
    public int LoadMixId { get; set; } // Foreign key referencing Id in LoadMixes
    public int GroupId { get; set; }   // New column, value from LoadMix.LoadMixId

    public int SelectedFieldId
    {
        get => _selectedFieldId;
        set
        {
            if (_selectedFieldId != value)
            {
                _selectedFieldId = value;

                // Trigger the event when the SelectedFieldId changes
                OnSelectedFieldIdChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public string FieldName { get; set; }
    public decimal FieldAverageRate { get; set; }
    public decimal FieldTotalGallons { get; set; }
    public decimal FieldAcres { get; set; }
    public int CustomerId { get; set; } // Foreign key linking to Customer

    // Navigation properties
    public Customer Customer { get; set; } // Navigation property to Customer
    public LoadMix LoadMix { get; set; }   // Navigation property to LoadMix

    // Unique list of fields for the dropdown - NotMapped because it's not part of EF
    [NotMapped]
    public List<Field> DropdownFields { get; set; } = new List<Field>();

    // Event handler to notify when SelectedFieldId changes
    public event EventHandler OnSelectedFieldIdChanged;

    [NotMapped]
    public bool IsCustomerApplied { get; set; }


}
