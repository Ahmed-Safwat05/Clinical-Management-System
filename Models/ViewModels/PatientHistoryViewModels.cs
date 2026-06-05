namespace ClinicManagementSystem.Models.ViewModels;

/// <summary>
/// Summary statistics for a patient's visit history
/// </summary>
public class PatientHistorySummaryDto
{
    public int PatientId { get; set; }
    public int TotalVisits { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public string? LastDoctorName { get; set; }
    public decimal TotalAmountSpent { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public bool HasHistory => TotalVisits > 0;
}

/// <summary>
/// Individual visit for clinical timeline
/// </summary>
public class VisitTimelineItemDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? DoctorName { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalPaid { get; set; }
    public VisitStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsVoided => Status == VisitStatus.Voided;
    public decimal RemainingBalance => Math.Max(0m, TotalPrice - TotalPaid);
    public bool IsPaidInFull => RemainingBalance <= 0m;
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
}

/// <summary>
/// Complete patient visit timeline
/// </summary>
public class PatientVisitTimelineDto
{
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public List<VisitTimelineItemDto> Visits { get; set; } = new List<VisitTimelineItemDto>();
    public int TotalVisitCount => Visits.Count;
}

/// <summary>
/// Previous visit (simplified for display in current visit)
/// </summary>
public class PreviousVisitDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? DoctorName { get; set; }
    public decimal TotalPrice { get; set; }
    public VisitStatus Status { get; set; }
    public bool IsVoided => Status == VisitStatus.Voided;
}
