namespace ClinicManagementSystem.Models.ViewModels;

public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Range(0, 999999999)]
    public decimal CostPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }

    public bool IsActive { get; set; }
}
