namespace ClinicManagementSystem.Models;

public class Appointment
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Range(1, int.MaxValue)]
    public int QueueNumber { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Waiting;

    public bool IsWalkIn { get; set; } = false;

    public Visit? Visit { get; set; }
    public bool IsDeleted { get; internal set; }
}
