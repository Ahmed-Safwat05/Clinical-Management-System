using System.Globalization;
using ClinicManagementSystem.Interfaces.Repositories;
using ClinicManagementSystem.Interfaces.Services;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Services;

public class SettingsService : ISettingsService
{
    private readonly IRepository<Setting> _settings;

    public SettingsService(IRepository<Setting> settings)
    {
        _settings = settings;
    }

    public async Task<string?> GetValue(string key)
    {
        var setting = (await _settings.FindAsync(x => x.Key == key)).FirstOrDefault();
        return setting?.Value;
    }

    public async Task<int> GetInt(string key)
    {
        var value = await GetValue(key);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    public async Task<decimal> GetDecimal(string key)
    {
        var value = await GetValue(key);
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result : 0m;
    }

    public async Task<bool> GetBool(string key)
    {
        var value = await GetValue(key);
        return bool.TryParse(value, out var result) && result;
    }

    public async Task SetValue(string key, string value)
    {
        var setting = (await _settings.FindAsync(x => x.Key == key)).FirstOrDefault();
        if (setting is null)
        {
            await _settings.AddAsync(new Setting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
            _settings.Update(setting);
        }

        await _settings.SaveChangesAsync();
    }
}
