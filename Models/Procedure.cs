namespace ClinicManagementSystem.Models;

public class Procedure
{
    public int Id { get; set; }

    [Required, StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 999999)]
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; } 

    public ICollection<VisitProcedure> VisitProcedures { get; set; } = new List<VisitProcedure>();
}
