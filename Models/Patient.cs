namespace ClinicManagementSystem.Models;

public class Patient
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Range(0, 120)]
    public int Age { get; set; }

    [Required]
    public Gender Gender { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<PatientMedicalHistoryEntry> MedicalHistoryEntries { get; set; } = new List<PatientMedicalHistoryEntry>();
}
