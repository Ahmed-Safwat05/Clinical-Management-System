namespace ClinicManagementSystem.Models.ViewModels;

public class AppointmentsIndexViewModel
{
    public DateTime SelectedDate { get; set; }
    public IReadOnlyList<DoctorQueueTabViewModel> DoctorQueues { get; set; } = Array.Empty<DoctorQueueTabViewModel>();
}

/// <summary>
/// Contains both scheduled and walk-in queues for a single doctor.
/// </summary>
public class DoctorQueueTabViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;

    // Scheduled Queue
    public int ScheduledWaitingCount { get; set; }
    public IReadOnlyList<AppointmentListItemViewModel> ScheduledQueue { get; set; } = Array.Empty<AppointmentListItemViewModel>();

    // Walk-In Queue
    public int WalkInWaitingCount { get; set; }
    public IReadOnlyList<AppointmentListItemViewModel> WalkInQueue { get; set; } = Array.Empty<AppointmentListItemViewModel>();
}

public class AppointmentListItemViewModel
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public int QueueNumber { get; set; }
    public bool IsWalkIn { get; set; }
    public bool CanCreateVisit { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;

    /// <summary>
    /// Returns formatted queue number: "1", "2" for scheduled or "W1", "W2" for walk-in.
    /// </summary>
    public string FormattedQueueNumber => IsWalkIn ? $"W{QueueNumber}" : QueueNumber.ToString();
}
