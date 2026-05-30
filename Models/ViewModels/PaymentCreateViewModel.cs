namespace ClinicManagementSystem.Models.ViewModels;

public class PaymentCreateViewModel
{
    public int VisitId { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "قيمة الدفعة يجب أن تكون أكبر من صفر")]
    public decimal Amount { get; set; }

    [StringLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    public decimal VisitTotalPrice { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
}
