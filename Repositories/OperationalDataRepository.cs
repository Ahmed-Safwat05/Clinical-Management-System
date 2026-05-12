using ClinicManagementSystem.Data;
using ClinicManagementSystem.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementSystem.Repositories;

public class OperationalDataRepository : IOperationalDataRepository
{
    private readonly ApplicationDbContext _context;

    public OperationalDataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task DeleteAllVisitsAsync()
    {
        var visitProcedures = await _context.VisitProcedures.IgnoreQueryFilters().ToListAsync();
        _context.VisitProcedures.RemoveRange(visitProcedures);

        var visits = await _context.Visits.IgnoreQueryFilters().ToListAsync();
        _context.Visits.RemoveRange(visits);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllAppointmentsAsync()
    {
        var appointments = await _context.Appointments.IgnoreQueryFilters().ToListAsync();
        _context.Appointments.RemoveRange(appointments);
        await _context.SaveChangesAsync();
    }
}
