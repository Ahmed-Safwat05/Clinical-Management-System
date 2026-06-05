using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Models.ViewModels;

public class PatientDetailsViewModel
{
    public Patient? Patient { get; set; }
    public PatientHistorySummaryDto? HistorySummary { get; set; }
    public PatientVisitTimelineDto? VisitTimeline { get; set; }
}
