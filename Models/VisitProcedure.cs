namespace ClinicManagementSystem.Models;

public class VisitProcedure
{
    public int VisitId { get; set; }
    public Visit? Visit { get; set; }

    public int ProcedureId { get; set; }
    public Procedure? Procedure { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
    public bool IsDeleted { get; internal set; }
}
