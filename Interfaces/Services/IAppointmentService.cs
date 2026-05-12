namespace ClinicManagementSystem.Interfaces.Services;

public interface IAppointmentService
{
    Task<IReadOnlyList<Appointment>> GetQueueByDateAsync(DateTime date);
    Task<Appointment?> GetDetailsAsync(int id);
    Task<Appointment> CreateAsync(int patientId, int doctorId, DateTime date);
    Task CancelAsync(int id);
    Task MarkDoneAsync(int id);

    // Queue management
    Task<IReadOnlyList<Appointment>> GetActiveQueueAsync(DateTime date);
    Task<IReadOnlyList<Appointment>> GetScheduledQueueByDoctorAsync(int doctorId, DateTime date);
    Task<IReadOnlyList<Appointment>> GetWalkInQueueByDoctorAsync(int doctorId, DateTime date);
}
