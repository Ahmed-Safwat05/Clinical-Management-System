namespace ClinicManagementSystem.DTOs.Visits
{
    public class VisitExcelDto
    {
        public int رقم_الزيارة { get; set; }
        public string المريض { get; set; } = string.Empty;
        public string الطبيب { get; set; } = string.Empty;
        public string التاريخ { get; set; } = string.Empty;
        public string التشخيص { get; set; } = string.Empty;
        public string التكلفة { get; set; } = string.Empty;
        public string الدفع_المستلم { get; set; } = string.Empty;
        public string المتبقي { get; set; } = string.Empty;
        public string حالة_الدفع { get; set; } = string.Empty;

    }
}
