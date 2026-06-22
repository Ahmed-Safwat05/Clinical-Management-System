namespace ClinicManagementSystem.DTOs.Financial
{
    public class FinancialReportFilterDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ReportType { get; set; } = "Daily"; // Daily / Monthly / Custom
    }
}
