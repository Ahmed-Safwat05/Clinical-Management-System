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

    public List<VisitProcedure> VisitProcedures { get; set; } = new List<VisitProcedure>();
}
