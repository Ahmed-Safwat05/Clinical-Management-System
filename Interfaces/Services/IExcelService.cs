namespace ClinicManagementSystem.Interfaces.Services
{
    public interface IExcelService
    {
        byte[] ExportExcel<T>(IEnumerable<T> data, string sheetName);
    }
}
