namespace ClinicManagementSystem.Models.ViewModels;

public class HomeViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TodayVisitsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public List<AppointmentDto> TodayAppointments { get; set; } = new();
    public List<DoctorLoadDto> DoctorLoads { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
    public List<RevenuePointDto> RevenueChart { get; set; } = new();
}

public class AppointmentDto
{
    public string Time { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
}

public class DoctorLoadDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public int LoadPercentage { get; set; }
}

public class NotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class RevenuePointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int VisitsCount { get; set; }
}
