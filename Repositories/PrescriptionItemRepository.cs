namespace ClinicManagementSystem.Repositories;

public class PrescriptionItemRepository : Repository<PrescriptionItem>, IPrescriptionItemRepository
{
    private readonly ApplicationDbContext _context;

    public PrescriptionItemRepository(ApplicationDbContext context) : base(context)
    {
        _context = context; 
    }

    public override void Delete(PrescriptionItem entity)
    {
        _context.PrescriptionItems.Remove(entity);
    }
}