namespace ClinicManagementSystem.Interfaces.Services;

public interface ISettingsService
{
    Task<string?> GetValue(string key);
    Task<int> GetInt(string key);
    Task<decimal> GetDecimal(string key);
    Task<bool> GetBool(string key);
    Task SetValue(string key, string value);
}
