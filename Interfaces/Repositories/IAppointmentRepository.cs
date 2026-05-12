namespace ClinicManagementSystem.Interfaces.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetByDateAsync(DateTime date);
    Task<Appointment?> GetDetailsAsync(int id);
    Task<int> GetNextQueueNumberAsync(int doctorId, DateTime date, bool isWalkIn);
    Task<int> CountByDateAsync(DateTime date);

    // Queue management
    Task<IReadOnlyList<Appointment>> GetActiveQueueAsync(DateTime date);
    Task<IReadOnlyList<Appointment>> GetScheduledQueueByDoctorAsync(int doctorId, DateTime date);
    Task<IReadOnlyList<Appointment>> GetWalkInQueueByDoctorAsync(int doctorId, DateTime date);
    Task<IReadOnlyList<Appointment>> GetAppointmentsAfterQueueNumberAsync(int doctorId, DateTime date, int queueNumber, bool isWalkIn);
}
