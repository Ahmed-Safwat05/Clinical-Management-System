namespace ClinicManagementSystem.Models.ViewModels;

public class PatientHistorySnapshotViewModel
{
    public PatientHistorySummaryDto? Summary { get; set; }
    public IReadOnlyList<PatientMedicalHistoryEntry> MedicalHistoryEntries { get; set; } = Array.Empty<PatientMedicalHistoryEntry>();
}
