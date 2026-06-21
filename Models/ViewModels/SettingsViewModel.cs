namespace ClinicManagementSystem.Models.ViewModels;

public class SettingsViewModel
{
    [Required, StringLength(120)]
    public string ClinicName { get; set; } = string.Empty;
    [StringLength(20)]
    public string ClinicPhone { get; set; } = string.Empty;
    [StringLength(200)]
    public string ClinicAddress { get; set; } = string.Empty;
    public string? ClinicLogoPath { get; set; } = string.Empty;
    [StringLength(500)]
    public string? ReceiptFooter { get; set; } = string.Empty;
    [Range(0, 999999)]
    public decimal DefaultExamPrice { get; set; }
    
    [Range(0, 999999)]
    public decimal MaxDiscount { get; set; }

    public bool AllowDiscount { get; set; }
    public IFormFile? LogoFile { get; set; }
    public IReadOnlyList<BackupInfoDto>? Backups { get; set; } = new List<BackupInfoDto>();
}
