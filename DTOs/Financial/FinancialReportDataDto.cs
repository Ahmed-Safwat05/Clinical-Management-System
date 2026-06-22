namespace ClinicManagementSystem.DTOs.Financial
{
    public class FinancialReportDataDto
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;

        // فترة التقرير
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // مؤشرات مالية وتفضيلية
        public decimal TotalRevenue { get; set; }
        public int TotalVisits { get; set; }
        public int TotalPayments { get; set; }
        public decimal AverageVisitRevenue { get; set; }
        public decimal OutstandingAmount { get; set; }

        // معلومات الإنشاء
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        // التفاصيل
        public List<FinancialTransactionDto> Transactions { get; set; } = new();
    }

    public class FinancialTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // Visit Payment / Product Purchase ...
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}