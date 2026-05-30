namespace ClinicManagementSystem.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentCreateViewModel?> BuildCreateModelAsync(int visitId);
    Task AddPaymentAsync(PaymentCreateViewModel model);
}
