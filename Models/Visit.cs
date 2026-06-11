namespace ClinicManagementSystem.Models;

public class Visit
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
    public decimal ExaminationPrice { get; set; }
    public decimal ProceduresPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; } 

    public bool Paid { get; set; }

    public decimal PaidAmount { get; set; }

    public VisitStatus Status { get; set; } = VisitStatus.Active;

    public DateTime? VoidedAt { get; set; }

    [StringLength(500)]
    public string? VoidReason { get; set; }

    public List<VisitProcedure> VisitProcedures { get; set; } = new List<VisitProcedure>();
    public List<Payment> Payments { get; set; } = new List<Payment>();
    public List<PrescriptionItem> Prescriptions { get; set; } = new List<PrescriptionItem>();
    public bool IsVoided => Status == VisitStatus.Voided;
    public decimal TotalPaid => PaidAmount;
    public decimal RemainingBalance => Math.Max(0m, TotalPrice - TotalPaid);
    public bool IsPaidInFull => RemainingBalance <= 0m;

    public bool IsDeleted { get; internal set; }
}
