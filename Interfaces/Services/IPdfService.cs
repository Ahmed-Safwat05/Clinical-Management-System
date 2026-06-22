using ClinicManagementSystem.DTOs.Financial;

namespace ClinicManagementSystem.Interfaces.Services
{
    public interface IPdfService
    {
        byte[] GenerateFinancialReport(FinancialReportDataDto reportData);
    }
}
