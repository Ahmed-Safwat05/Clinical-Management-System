namespace ClinicManagementSystem.Models.ViewModels;

public class VisitCreateViewModel
{
    public int? AppointmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "اختار مريض")]
    public int PatientId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "اختار طبيب")]
    public int DoctorId { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Range(0, 999999)]
    public decimal PaidAmount { get; set; }

    public bool Paid { get; set; }
    public decimal ExaminationPrice { get; set; }
    public decimal ProceduresPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }

    public List<VisitProcedureInput> Procedures { get; set; } = new();
    public IEnumerable<SelectListItem> Patients { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> Doctors { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> AvailableProcedures { get; set; } = Enumerable.Empty<SelectListItem>();
    public IReadOnlyList<ProcedureOptionViewModel> AvailableProcedureOptions { get; set; } = Array.Empty<ProcedureOptionViewModel>();
}

public class VisitProcedureInput
{
    public int? ProcedureId { get; set; }
    public int? Quantity { get; set; } = 1;
}

public class ProcedureOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
