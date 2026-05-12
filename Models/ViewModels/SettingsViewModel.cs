namespace ClinicManagementSystem.Models.ViewModels;

public class SettingsViewModel
{
    [Required, StringLength(120)]
    public string ClinicName { get; set; } = string.Empty;

    [Range(0, 999999)]
    public decimal DefaultExamPrice { get; set; }

    [Range(0, 999999)]
    public decimal MaxDiscount { get; set; }

    public bool AllowDiscount { get; set; }
}
