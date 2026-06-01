namespace ClinicManagementSystem.Interfaces.Services;

public interface IVisitService
{
    Task<IReadOnlyList<Visit>> GetRecentAsync();
    Task<Visit?> GetDetailsAsync(int id);
    Task<VisitCreateViewModel?> BuildFromAppointmentAsync(int appointmentId);
    Task CreateAsync(VisitCreateViewModel model);
    Task VoidAsync(int id, string? reason = null);
    Task DeleteAsync(int id);
}
