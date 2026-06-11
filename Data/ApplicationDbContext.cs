namespace ClinicManagementSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Procedure> Procedures => Set<Procedure>();
    public DbSet<VisitProcedure> VisitProcedures => Set<VisitProcedure>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<VisitProductConsumption> VisitProductConsumptions => Set<VisitProductConsumption>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PatientMedicalHistoryEntry> PatientMedicalHistoryEntries => Set<PatientMedicalHistoryEntry>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VisitProcedure>()
            .HasKey(x => new { x.VisitId, x.ProcedureId });

        modelBuilder.Entity<Patient>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Doctor>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Appointment>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Visit>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<VisitProcedure>()
            .HasQueryFilter(x => !x.IsDeleted);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(x => !x.IsDeleted);


        modelBuilder.Entity<AppUser>()
            .HasQueryFilter(x => x.IsActive);

        modelBuilder.Entity<Procedure>()
            .Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Visit>()
            .Property(x => x.ExaminationPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Visit>()
            .Property(x => x.ProceduresPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Visit>()
            .Property(x => x.Discount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Visit>()
            .Property(x => x.TotalPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Visit>()
            .Property(x => x.PaidAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Visit>()
            .Property(x => x.Status)
            .HasDefaultValue(VisitStatus.Active);

        modelBuilder.Entity<Visit>()
            .Property(x => x.VoidReason)
            .HasMaxLength(500);
        modelBuilder.Entity<Visit>()
            .HasIndex(x => x.Date);

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.VisitId);
            entity.HasIndex(x => x.CreatedAt);

            entity.HasOne(x => x.Visit)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.VisitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VisitProcedure>()
            .Property(x => x.SubTotal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Setting>()
            .HasIndex(x => x.Key)
            .IsUnique();

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.Username)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.EntityName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.IpAddress)
                .HasMaxLength(45);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.Username);
            entity.HasIndex(x => x.ActionType);
        });

        modelBuilder.Entity<PatientMedicalHistoryEntry>(entity =>
        {
            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.PatientId);
            entity.HasIndex(x => x.CreatedAt);

            entity.HasOne(x => x.Patient)
                .WithMany(x => x.MedicalHistoryEntries)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VisitProcedure>()
            .HasOne(x => x.Visit)
            .WithMany(x => x.VisitProcedures)
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VisitProcedure>()
            .HasOne(x => x.Procedure)
            .WithMany(x => x.VisitProcedures)
            .HasForeignKey(x => x.ProcedureId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasIndex(x => new { x.DoctorId, x.Date, x.QueueNumber })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visit>()
            .HasOne(x => x.Patient)
            .WithMany(x => x.Visits)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visit>()
            .HasOne(x => x.Doctor)
            .WithMany(x => x.Visits)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visit>()
            .HasOne(x => x.Appointment)
            .WithOne(x => x.Visit)
            .HasForeignKey<Visit>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DoctorSchedule>()
            .HasIndex(x => new { x.DoctorId, x.DayOfWeek, x.StartTime, x.EndTime });

        modelBuilder.Entity<Product>()
            .HasQueryFilter(x => x.IsActive);

        modelBuilder.Entity<Product>()
            .Property(x => x.CostPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Procedure>().HasData(
            new Procedure { Id = 1, Name = "Medical Consultation", Price = 50m },
            new Procedure { Id = 2, Name = "Follow-up Visit", Price = 25m },
            new Procedure { Id = 3, Name = "Basic Checkup", Price = 40m });

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Username)
            .IsUnique();

        modelBuilder.Entity<StockTransaction>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransaction>()
            .HasOne(x => x.Visit)
            .WithMany()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<VisitProductConsumption>()
            .HasOne(x => x.Visit)   
            .WithMany()
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VisitProductConsumption>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VisitProductConsumption>()
            .HasOne(x => x.StockTransaction)
            .WithMany()
            .HasForeignKey(x => x.StockTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Setting>().HasData(
            new Setting { Id = 1, Key = "ClinicName", Value = "عيادة الخير" },
            new Setting { Id = 2, Key = "DefaultExamPrice", Value = "100" },
            new Setting { Id = 3, Key = "MaxDiscount", Value = "50" },
            new Setting { Id = 4, Key = "AllowDiscount", Value = "true" });
    }
}
