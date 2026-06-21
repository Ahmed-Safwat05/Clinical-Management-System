namespace ClinicManagementSystem.Services;

public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _context;
    private readonly string _dbPath;
    private readonly string _backupFolder;

    public BackupService(ApplicationDbContext context)
    {
        _context = context;
        // جلب مسار الـ DB الحالي المتطابق مع الـ Program.cs
        _dbPath = Path.Combine(AppContext.BaseDirectory, "clinic.db");
        _backupFolder = Path.Combine(AppContext.BaseDirectory, "Backups");

        if (!Directory.Exists(_backupFolder))
        {
            Directory.CreateDirectory(_backupFolder);
        }
    }

    public async Task<string> CreateBackupAsync()
    {
        // لضمان حفظ أي بيانات معلقة في الـ Memory إلى الهارد قبل النسخ
        await _context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(FULL);");
        await _context.Database.ExecuteSqlRawAsync("VACUUM;");

        string timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        string backupFileName = $"backup_{timestamp}.db";
        string destPath = Path.Combine(_backupFolder, backupFileName);

        // نسخ الملف بأمان
        File.Copy(_dbPath, destPath, overwrite: true);

        // تنفيذ التنظيف التلقائي للحفاظ على آخر 10 نسخ فقط
        ApplyRetentionPolicy();

        return backupFileName;
    }

    public async Task<IReadOnlyList<BackupInfoDto>> GetBackupsAsync()
    {
        var directory = new DirectoryInfo(_backupFolder);
        var files = directory.GetFiles("backup_*.db")
                             .OrderByDescending(f => f.CreationTime)
                             .Select(f => new BackupInfoDto
                             {
                                 FileName = f.Name,
                                 FullPath = f.FullName,
                                 FileSizeInBytes = f.Length,
                                 CreatedAt = f.CreationTime
                             }).ToList();

        return await Task.FromResult(files);
    }

    public async Task RestoreBackupAsync(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        string backupFilePath = Path.Combine(_backupFolder, fileName);

        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود.");
        }

        // 🎯 الحماية الفيدرالية: عمل نسخة احتياطية طارئة من الوضع الحالي المكسور قبل الـ Restore
        string emergencyBackupPath = Path.Combine(_backupFolder, $"emergency_before_restore_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.db");
        File.Copy(_dbPath, emergencyBackupPath, true);

        // 🎯 1. تفجير وإغلاق كل الـ Connection Pools بتاعة الـ SQLite لفك الكاش تماماً
        var connection = _context.Database.GetDbConnection();
        if (connection.State == System.Data.ConnectionState.Open)
        {
            await connection.CloseAsync();
        }

        // إجبار الـ SQLite على مسح الـ Connection Pooling
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();
     
        // مسح الكاش
        var walFile = _dbPath + "-wal";
        var shmFile = _dbPath + "-shm";

        if (File.Exists(walFile)) File.Delete(walFile);
        if (File.Exists(shmFile)) File.Delete(shmFile);

        // 🎯 3. محاولة الاستبدال مع تكرار المحاولة (Retry Mechanism) لو الملف مهنج
        int retries = 5;
        while (retries > 0)
        {
            try
            {
                File.Copy(backupFilePath, _dbPath, overwrite: true);
                break; // نجحت العملية! نخرج من الـ Loop
            }
            catch (IOException)
            {
                retries--;
                if (retries == 0) throw;
                await Task.Delay(200); // انتظر 200 مللي ثانية وجرب تاني
            }
        }
    }

    public async Task DeleteBackupAsync(string fileName)
    {
        string filePath = Path.Combine(_backupFolder, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        await Task.CompletedTask;
    }

    private void ApplyRetentionPolicy()
    {
        var directory = new DirectoryInfo(_backupFolder);
        var files = directory.GetFiles("backup_*.db")
                             .OrderByDescending(f => f.CreationTime)
                             .Skip(10) // تخطي أول 10 نسخ حديثة
                             .ToList();

        foreach (var file in files)
        {
            file.Delete(); // مسح النسخ الزائدة القديمة تلقائياً
        }
    }
}