using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManagementSystem.Models;

[Table("Products")]
public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }

    [StringLength(50)]
    public string Unit { get; set; } = "وحدة";

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
