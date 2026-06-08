namespace ClinicManagementSystem.Repositories;

public class DoctorRepository : Repository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Doctor>> GetWithSchedulesAsync()
    {
        return await Context.Doctors
            .Include(x => x.Schedules)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Doctor>> GetDeletedAsync()
    {
        return await Context.Doctors
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<Doctor?> GetDeletedByIdAsync(int id)
    {
        return Context.Doctors
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);
    }

    public Task<int> CountAsync()
    {
        return Context.Doctors.CountAsync();
    }
}
