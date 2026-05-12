namespace ClinicManagementSystem.Interfaces.Services;

public interface IOperationalDataService
{
    Task DeleteAllVisitsAsync();
    Task DeleteAllAppointmentsAsync();
    Task ResetOperationalDataAsync();
}
