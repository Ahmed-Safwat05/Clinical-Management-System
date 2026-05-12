namespace ClinicManagementSystem.Models.ViewModels;

public class DashboardViewModel
{
    public DateTime Date { get; set; }
    public int PatientsToday { get; set; }
    public decimal IncomeToday { get; set; }
    public int AppointmentsToday { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public decimal TotalIncome { get; set; }
    public IReadOnlyList<DailyReportPoint> DailyReports { get; set; } = Array.Empty<DailyReportPoint>();
}

public class DailyReportPoint
{
    public DateTime Date { get; set; }
    public int PatientCount { get; set; }
    public int AppointmentCount { get; set; }
    public decimal Income { get; set; }
}
