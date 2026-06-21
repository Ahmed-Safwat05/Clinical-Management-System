namespace ClinicManagementSystem.Interfaces.Services;
public class BackupInfoDto
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    // حجم الملف بشكل مقروء للدكتور (مثال: 2.4 MB)
    public string DisplaySize => $"{(FileSizeInBytes / 1024.0 / 1024.0):F2} MB";
}

public interface IBackupService
{
    Task<string> CreateBackupAsync();
    Task<IReadOnlyList<BackupInfoDto>> GetBackupsAsync();
    Task RestoreBackupAsync(string fileName);
    Task DeleteBackupAsync(string fileName);
}
