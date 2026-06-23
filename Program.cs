using ClinicManagementSystem.Middleware;
using QuestPDF.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
// 🎯 تجميع الـ Logging في مكان واحد وتنظيف التكرار
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var dbPath = Path.Combine(AppContext.BaseDirectory, "clinic.db");
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")))
    .SetApplicationName("CharityClinicCms");

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // 🎯 زيادة أمان الـ Cookie ضد هجمات الـ XSS والـ CSRF
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IProcedureRepository, ProcedureRepository>();
builder.Services.AddScoped<IOperationalDataRepository, OperationalDataRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<IVisitProductConsumptionRepository, VisitProductConsumptionRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IPatientMedicalHistoryRepository, PatientMedicalHistoryRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPrescriptionItemRepository, PrescriptionItemRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IOperationalDataService, OperationalDataService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IProcedureService, ProcedureService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockManagementService, StockManagementService>();
builder.Services.AddScoped<IVisitConsumptionService, VisitConsumptionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPatientMedicalHistoryService, PatientMedicalHistoryService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPatientHistoryService, PatientHistoryService>();
builder.Services.AddScoped<IPrescriptionItemService, PrescriptionItemService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IPdfService, PdfService>();
var app = builder.Build();

// Seed database with default users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // 1. عمل الـ Migrate أوتوماتيكياً لإنشاء الجداول (لـ SQLite)
        await dbContext.Database.MigrateAsync();

        // 2. تنظيف الـ Audit Logs القديمة (تم رفعها لـ 90 يوم لأمان الداتا)
        var retentionDate = DateTime.UtcNow.AddDays(-90);
        var oldLogs = dbContext.AuditLogs.Where(log => log.CreatedAt < retentionDate);

        if (oldLogs.Any())
        {
            dbContext.AuditLogs.RemoveRange(oldLogs);
            await dbContext.SaveChangesAsync();

            // ضغط ملف الـ SQLite لتقليص المساحة على الهارد
            await dbContext.Database.ExecuteSqlRawAsync("VACUUM;");
        }
    }
    catch (Exception ex)
    {
        // استخدام الـ Console المؤقت في الـ Startup
        Console.WriteLine($"An error occurred during startup/migration: {ex.Message}");
    }
}


// الجزء الخاص بالداتا التجريبية (الديمو) - يعمل في الـ Development فقط
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        if (app.Environment.IsDevelopment())
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            DbInitializer.Seed(context);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حصلت مشكلة أثناء تنزيل الداتا التجريبية.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new[] { "ar-EG", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseHttpsRedirection();
app.UseRequestLocalization(localizationOptions);

app.UseStaticFiles();
app.UseRouting();

app.UseMiddleware<SetupMiddleware>();
app.UseMiddleware<LicenseMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// كود الفتح التلقائي للبراوزر - معدل ليكون ديناميكي تماماً بناءً على البورت الفعلي للـ Server
app.Lifetime.ApplicationStarted.Register(() =>
{
    if (!app.Environment.IsDevelopment()) // يشتغل في البابلش بس
    {
        try
        {
            // 🎯 جلب الـ URL الفعلي اللي الـ Kestrel اشتغل عليه (سواء 5000 أو غيره)
            var serverUrl = app.Urls.FirstOrDefault() ?? "http://localhost:5000";

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = serverUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open browser automatically: {ex.Message}");
        }
    }
});

app.Run();