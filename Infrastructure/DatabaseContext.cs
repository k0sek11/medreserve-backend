using Medreserve.Features.Appointment;
using Medreserve.Features.AppointmentType;
using Medreserve.Features.Clinic;
using Medreserve.Features.Doctor;
using Medreserve.Features.Notification;
using Medreserve.Features.Patient;
using Medreserve.Features.Payment;
using Medreserve.Features.Specialization;
using Medreserve.Features.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Infrastructure;

public class DatabaseContext(IConfiguration configuration) : IdentityDbContext<User>
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicDoctor> ClinicDoctors => Set<ClinicDoctor>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorSpecialization> DoctorSpecializations => Set<DoctorSpecialization>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
    public DbSet<DoctorAppointmentType> DoctorAppointmentTypes => Set<DoctorAppointmentType>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OfflinePaymentApproval> OfflinePaymentApprovals => Set<OfflinePaymentApproval>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var connectionString = configuration.GetConnectionString("Default");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(x => x.PatientId);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.Pesel).IsUnique();

            entity
                .HasOne(x => x.User)
                .WithOne(x => x.PatientProfile)
                .HasForeignKey<Patient>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(x => x.DoctorId);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.LicenseNumber).IsUnique();
            entity.Property(x => x.LicenseNumber).IsRequired();

            entity
                .HasOne(x => x.User)
                .WithOne(x => x.DoctorProfile)
                .HasForeignKey<Doctor>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(x => x.ClinicId);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Address).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
        });

        modelBuilder.Entity<ClinicDoctor>(entity =>
        {
            entity.HasKey(x => new { x.ClinicId, x.DoctorId });
            entity.Property(x => x.IsOwner).IsRequired();

            entity
                .HasOne(x => x.Clinic)
                .WithMany(x => x.ClinicDoctors)
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Doctor)
                .WithMany(x => x.ClinicDoctors)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(x => x.SpecializationId);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<DoctorSpecialization>(entity =>
        {
            entity.HasKey(x => new { x.DoctorId, x.SpecializationId });

            entity
                .HasOne(x => x.Doctor)
                .WithMany(x => x.DoctorSpecializations)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Specialization)
                .WithMany(x => x.DoctorSpecializations)
                .HasForeignKey(x => x.SpecializationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DoctorSchedule>(entity =>
        {
            entity.HasKey(x => x.ScheduleId);
            entity.Property(x => x.DayOfWeek).IsRequired();
            entity.Property(x => x.StartTime).IsRequired();
            entity.Property(x => x.EndTime).IsRequired();
            entity.Property(x => x.ValidFrom).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();

            entity
                .HasOne(x => x.Doctor)
                .WithMany(x => x.DoctorSchedules)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppointmentType>(entity =>
        {
            entity.HasKey(x => x.AppointmentTypeId);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.BasePrice).IsRequired();
            entity.Property(x => x.DurationMinutes).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<DoctorAppointmentType>(entity =>
        {
            entity.HasKey(x => new { x.DoctorId, x.AppointmentTypeId });

            entity
                .HasOne(x => x.Doctor)
                .WithMany(x => x.DoctorAppointmentTypes)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.AppointmentType)
                .WithMany(x => x.DoctorAppointmentTypes)
                .HasForeignKey(x => x.AppointmentTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(x => x.AppointmentId);
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.TimeSlotId).IsUnique();

            entity
                .HasOne(x => x.Patient)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Doctor)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.AppointmentType)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.AppointmentTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.Property(x => x.Amount).IsRequired();
            entity.Property(x => x.Currency).IsRequired();
            entity.Property(x => x.Method).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();

            entity
                .HasOne(x => x.Appointment)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OfflinePaymentApproval>(entity =>
        {
            entity.HasKey(x => x.ApprovalId);
            entity.Property(x => x.Decision).IsRequired();
            entity.Property(x => x.DecisionDate).IsRequired();
            entity.HasIndex(x => x.PaymentId).IsUnique();

            entity
                .HasOne(x => x.Payment)
                .WithOne(x => x.OfflinePaymentApproval)
                .HasForeignKey<OfflinePaymentApproval>(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.ApprovedByUser)
                .WithMany(x => x.OfflinePaymentApprovals)
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(x => x.NotificationId);
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Subject).IsRequired();
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();

            entity
                .HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Appointment)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
