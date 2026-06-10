namespace ClinicManagementSystem.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Patient>> SearchAsync(string? searchTerm)
    {
        var query = Context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.Phone.Contains(searchTerm) ||
                x.MedicalHistoryEntries.Any(entry =>
                    entry.Title.Contains(searchTerm) ||
                    entry.Description.Contains(searchTerm)));
        }

        return await query.OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<Patient>> GetDeletedAsync()
    {
        return await Context.Patients
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<Patient?> GetDeletedByIdAsync(int id)
    {
        return Context.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);
    }

    public Task<int> CountAsync()
    {
        return Context.Patients.CountAsync();
    }
}
