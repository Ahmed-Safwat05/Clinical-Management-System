namespace ClinicManagementSystem.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Appointment>> GetByDateAsync(DateTime date)
    {
        var targetDate = date.Date;

        return await Context.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .AsNoTracking()
            .Where(x => x.Date.Date == targetDate)
            .OrderBy(x => x.Doctor!.Name)
            .ThenBy(x => x.QueueNumber)
            .ToListAsync();
    }

    public Task<Appointment?> GetDetailsAsync(int id)
    {
        return Context.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<int> GetNextQueueNumberAsync(int doctorId, DateTime date, bool isWalkIn)
    {
        var targetDate = date.Date;
        var lastNumber = await Context.Appointments
            .Where(x => x.DoctorId == doctorId && 
                        x.Date.Date == targetDate && 
                        x.IsWalkIn == isWalkIn &&
                        x.Status != AppointmentStatus.Cancelled)
            .MaxAsync(x => (int?)x.QueueNumber);

        return (lastNumber ?? 0) + 1;
    }

    public Task<int> CountByDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return Context.Appointments.CountAsync(x => x.Date.Date == targetDate);
    }

    /// <summary>
    /// Gets all active (non-cancelled) appointments for a given date.
    /// Used for queue display screens.
    /// </summary>
    public async Task<IReadOnlyList<Appointment>> GetActiveQueueAsync(DateTime date)
    {
        var targetDate = date.Date;

        return await Context.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .AsNoTracking()
            .Where(x => x.Date.Date == targetDate && x.Status != AppointmentStatus.Cancelled)
            .OrderBy(x => x.Doctor!.Name)
            .ThenBy(x => x.IsWalkIn)  // Scheduled first, then Walk-In
            .ThenBy(x => x.QueueNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets scheduled queue (not walk-in) for a specific doctor on a given date.
    /// Only includes waiting appointments.
    /// </summary>
    public async Task<IReadOnlyList<Appointment>> GetScheduledQueueByDoctorAsync(int doctorId, DateTime date)
    {
        var targetDate = date.Date;

        return await Context.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId &&
                        x.Date.Date == targetDate &&
                        x.IsWalkIn == false &&
                        x.Status == AppointmentStatus.Waiting)
            .OrderBy(x => x.QueueNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets walk-in queue for a specific doctor on a given date.
    /// Only includes waiting appointments.
    /// </summary>
    public async Task<IReadOnlyList<Appointment>> GetWalkInQueueByDoctorAsync(int doctorId, DateTime date)
    {
        var targetDate = date.Date;

        return await Context.Appointments
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .AsNoTracking()
            .Where(x => x.DoctorId == doctorId &&
                        x.Date.Date == targetDate &&
                        x.IsWalkIn == true &&
                        x.Status == AppointmentStatus.Waiting)
            .OrderBy(x => x.QueueNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all appointments with queue numbers greater than the cancelled one.
    /// Used for reordering queue after cancellation.
    /// Only includes appointments with same doctor, date, walk-in status, and waiting status.
    /// </summary>
    public async Task<IReadOnlyList<Appointment>> GetAppointmentsAfterQueueNumberAsync(
        int doctorId, 
        DateTime date, 
        int queueNumber, 
        bool isWalkIn)
    {
        var targetDate = date.Date;

        return await Context.Appointments
            .Where(x => x.DoctorId == doctorId &&
                        x.Date.Date == targetDate &&
                        x.IsWalkIn == isWalkIn &&
                        x.Status == AppointmentStatus.Waiting &&
                        x.QueueNumber > queueNumber)
            .ToListAsync();
    }
}
