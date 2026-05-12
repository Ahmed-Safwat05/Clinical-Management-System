namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IOperationalDataRepository
{
    Task DeleteAllVisitsAsync();
    Task DeleteAllAppointmentsAsync();
}
