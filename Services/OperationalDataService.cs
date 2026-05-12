using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Interfaces.Services;

namespace ClinicManagementSystem.Services;

public class OperationalDataService : IOperationalDataService
{
    private readonly IOperationalDataRepository _operationalData;

    public OperationalDataService(IOperationalDataRepository operationalData)
    {
        _operationalData = operationalData;
    }

    public Task DeleteAllVisitsAsync()
    {
        return _operationalData.DeleteAllVisitsAsync();
    }

    public Task DeleteAllAppointmentsAsync()
    {
        return _operationalData.DeleteAllAppointmentsAsync();
    }

    public async Task ResetOperationalDataAsync()
    {
        await _operationalData.DeleteAllVisitsAsync();
        await _operationalData.DeleteAllAppointmentsAsync();
    }
}
