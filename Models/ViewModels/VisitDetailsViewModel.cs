namespace ClinicManagementSystem.Models.ViewModels;

public class VisitDetailsViewModel
{
    public Visit Visit { get; set; } = null!;
    public IReadOnlyList<VisitProductConsumption> ProductConsumptions { get; set; } = Array.Empty<VisitProductConsumption>();
    public IEnumerable<SelectListItem> AvailableProducts { get; set; } = Enumerable.Empty<SelectListItem>();
    public decimal TotalConsumptionCost { get; set; }
    public IReadOnlyList<PatientMedicalHistoryEntry> PatientMedicalHistory { get; set; } = Array.Empty<PatientMedicalHistoryEntry>();
}
