namespace ClinicManagementSystem.Models.ViewModels;

public class PatientMedicalHistoryViewModel
{
    public Patient Patient { get; set; } = null!;
    public IReadOnlyList<PatientMedicalHistoryEntry> Entries { get; set; } = Array.Empty<PatientMedicalHistoryEntry>();
    public IReadOnlyList<PatientMedicalHistoryEntry> SummaryEntries { get; set; } = Array.Empty<PatientMedicalHistoryEntry>();
}

public class PatientMedicalHistoryEntryFormViewModel
{
    public int? Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "عنوان السجل المرضي مطلوب")]
    [StringLength(200, ErrorMessage = "العنوان يجب ألا يتجاوز 200 حرف")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف السجل المرضي مطلوب")]
    [StringLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
    public string Description { get; set; } = string.Empty;
}
