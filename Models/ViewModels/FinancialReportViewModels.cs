namespace ClinicManagementSystem.Models.ViewModels;

public class FinancialSummaryDto
{
    public decimal BilledRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
    public decimal OutstandingBalance => BilledRevenue - CollectedRevenue;
}

public class PaymentStatusAnalyticsDto
{
    public int FullyPaidVisitsCount { get; set; }
    public int PartiallyPaidVisitsCount { get; set; }
    public int UnpaidVisitsCount { get; set; }
}

public class TopDebtorDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal OutstandingBalance { get; set; }
}
