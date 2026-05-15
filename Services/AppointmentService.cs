namespace ClinicManagementSystem.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointments;

    public AppointmentService(IAppointmentRepository appointments)
    {
        _appointments = appointments;
    }

    public Task<IReadOnlyList<Appointment>> GetQueueByDateAsync(DateTime date) => 
        _appointments.GetByDateAsync(date);

    public Task<Appointment?> GetDetailsAsync(int id) => 
        _appointments.GetDetailsAsync(id);

    public async Task<Appointment> CreateAsync(int patientId, int doctorId, DateTime date)
    {
        var appointmentDate = date.Date;
        var isWalkIn = appointmentDate == DateTime.Today;
        var queueNumber = await _appointments.GetNextQueueNumberAsync(doctorId, appointmentDate, isWalkIn);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Date = date,
            QueueNumber = queueNumber,
            Status = AppointmentStatus.Waiting,
            IsWalkIn = isWalkIn
        };

        await _appointments.AddAsync(appointment);
        await _appointments.SaveChangesAsync();

        return appointment;
    }

    /// <summary>
    /// Cancels an appointment and reorders the queue.
    /// 
    /// Process:
    /// 1. Mark appointment as cancelled (don't delete)
    /// 2. Get all waiting appointments after this one (same doctor, date, walk-in status)
    /// 3. Decrement their queue numbers by 1
    /// 4. Save changes
    /// </summary>
    public async Task CancelAsync(int id)
    {
        var appointment = await _appointments.GetByIdAsync(id);

        // Cannot cancel if not found or already done
        if (appointment is null || appointment.Status == AppointmentStatus.Done)
        {
            return;
        }

        var cancelledQueueNumber = appointment.QueueNumber;
        var isWalkIn = appointment.IsWalkIn;
        var doctorId = appointment.DoctorId;
        var appointmentDate = appointment.Date;

        var nextNumber = await _appointments.GetNextQueueNumberAsync(
        appointment.DoctorId,
        appointment.Date.Date,
        appointment.IsWalkIn);

        appointment.QueueNumber = nextNumber + 80;
        // Mark as cancelled (don't delete from database)
        appointment.Status = AppointmentStatus.Cancelled;
        _appointments.Update(appointment);

        // Get all appointments that need reordering
        // Only reorder appointments with:
        // - Same doctor
        // - Same date
        // - Same walk-in status (don't mix scheduled with walk-in)
        // - Status = Waiting
        // - Queue number > cancelled queue number
        var affectedAppointments = await _appointments.GetAppointmentsAfterQueueNumberAsync(
            doctorId,
            appointmentDate,
            cancelledQueueNumber,
            isWalkIn);

        // Reorder queue numbers
        foreach (var item in affectedAppointments)
        {
            item.QueueNumber--;
            _appointments.Update(item);
        }

        await _appointments.SaveChangesAsync();
    }

    public async Task MarkDoneAsync(int id)
    {
        var appointment = await _appointments.GetByIdAsync(id);
        if (appointment is null)
        {
            return;
        }

        appointment.Status = AppointmentStatus.Done;
        _appointments.Update(appointment);
        await _appointments.SaveChangesAsync();
    }

    /// <summary>
    /// Gets all active (non-cancelled) appointments for a given date.
    /// Filtered for queue display screens.
    /// </summary>
    public Task<IReadOnlyList<Appointment>> GetActiveQueueAsync(DateTime date) =>
        _appointments.GetActiveQueueAsync(date);

    /// <summary>
    /// Gets scheduled queue for a specific doctor on a given date.
    /// Only includes waiting appointments (not walk-in).
    /// </summary>
    public Task<IReadOnlyList<Appointment>> GetScheduledQueueByDoctorAsync(int doctorId, DateTime date) =>
        _appointments.GetScheduledQueueByDoctorAsync(doctorId, date);

    /// <summary>
    /// Gets walk-in queue for a specific doctor on a given date.
    /// Only includes waiting appointments that are walk-in.
    /// </summary>
    public Task<IReadOnlyList<Appointment>> GetWalkInQueueByDoctorAsync(int doctorId, DateTime date) =>
        _appointments.GetWalkInQueueByDoctorAsync(doctorId, date);
}
