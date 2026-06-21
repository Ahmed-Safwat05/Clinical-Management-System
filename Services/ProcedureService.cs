namespace ClinicManagementSystem.Services;

public class ProcedureService : IProcedureService
{
    private readonly IProcedureRepository _procedures;

    public ProcedureService(IProcedureRepository procedures)
    {
        _procedures = procedures;
    }

    public Task<IReadOnlyList<Procedure>> GetAllAsync() => _procedures.GetOrderedAsync();

    public Task<Procedure?> GetByIdAsync(int id) => _procedures.GetByIdAsync(id);

    public async Task CreateAsync(Procedure procedure)
    {
        await _procedures.AddAsync(procedure);
        await _procedures.SaveChangesAsync();
    }

    public async Task UpdateAsync(Procedure procedure)
    {
        _procedures.Update(procedure);
        await _procedures.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var procedure = await _procedures.GetByIdAsync(id); // أو حسب طريقة جلب الإجراء عندك
        if (procedure != null)
        {
            // 🎯 بدل الحذف الفعلي اللي بيضرب القيد، بنعمل علم إنه محذوف
            // بحيث ما يظهرش للدكتور في قائمة الإجراءات الجديدة، بس يفضل مقروء في الزيارات القديمة
            procedure.IsDeleted = true;


            _procedures.Update(procedure);
            await _procedures.SaveChangesAsync();
        }
    }
}
