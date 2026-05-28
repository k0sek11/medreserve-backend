using Medreserve.Features.Appointment;
using Medreserve.Features.AppointmentType;
using Medreserve.Features.Clinic;
using Medreserve.Features.Doctor;
using Medreserve.Features.Geography;
using Medreserve.Features.Notification;
using Medreserve.Features.Payment;
using Medreserve.Features.Specialization;
using Medreserve.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Medreserve.Infrastructure;

public class DatabaseContext(IConfiguration configuration) : IdentityDbContext<User>
{
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicDoctor> ClinicDoctors => Set<ClinicDoctor>();
    public DbSet<City> Cities => Set<City>();
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
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasData(
                new City { CityId = 1, Name = "Warszawa", District = "Śródmieście", Voivodeship = "Mazowieckie" },
                new City { CityId = 2, Name = "Kraków", District = "Stare Miasto", Voivodeship = "Małopolskie" },
                new City { CityId = 3, Name = "Łódź", District = "Śródmieście", Voivodeship = "Łódzkie" },
                new City { CityId = 4, Name = "Wrocław", District = "Stare Miasto", Voivodeship = "Dolnośląskie" },
                new City { CityId = 5, Name = "Poznań", District = "Stare Miasto", Voivodeship = "Wielkopolskie" }
             );
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.BirthDate).HasColumnType("date");
            entity.Property(x => x.Gender);
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
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
            entity.Property(x => x.StreetAddress).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();
            entity.Property(x => x.CityId).IsRequired();

            entity
                .HasOne(x => x.City)
                .WithMany(x => x.Clinics)
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasData(
                new Specialization { SpecializationId = 1, Name = "Alergolog", Description = null },
                new Specialization { SpecializationId = 2, Name = "Anestezjolog", Description = null },
                new Specialization { SpecializationId = 3, Name = "Chirurg ogólny", Description = null },
                new Specialization { SpecializationId = 4, Name = "Internista", Description = null },
                new Specialization { SpecializationId = 5, Name = "Dermatolog", Description = null },
                new Specialization { SpecializationId = 6, Name = "Diabetolog", Description = null },
                new Specialization { SpecializationId = 7, Name = "Endokrynolog", Description = null },
                new Specialization { SpecializationId = 8, Name = "Gastroenterolog", Description = null },
                new Specialization { SpecializationId = 9, Name = "Ginekolog", Description = null },
                new Specialization { SpecializationId = 10, Name = "Kardiolog", Description = null },
                new Specialization { SpecializationId = 11, Name = "Lekarz medycyny pracy", Description = null },
                new Specialization { SpecializationId = 12, Name = "Lekarz medycyny rodzinnej", Description = null },
                new Specialization { SpecializationId = 13, Name = "Neurolog", Description = null },
                new Specialization { SpecializationId = 14, Name = "Okulista", Description = null },
                new Specialization { SpecializationId = 15, Name = "Onkolog", Description = null },
                new Specialization { SpecializationId = 16, Name = "Ortopeda", Description = null },
                new Specialization { SpecializationId = 17, Name = "Pediatra", Description = null },
                new Specialization { SpecializationId = 18, Name = "Psychiatra", Description = null },
                new Specialization { SpecializationId = 19, Name = "Pulmonolog", Description = null },
                new Specialization { SpecializationId = 20, Name = "Urolog", Description = null }
            );
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
            entity.Property(x => x.ClinicId);
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

            entity.HasIndex(x => x.ClinicId);

            entity
                .HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppointmentType>(entity =>
        {
            entity.HasKey(x => x.AppointmentTypeId);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.BasePrice).IsRequired();
            entity.Property(x => x.DurationMinutes).IsRequired();
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
            entity.Property(x => x.AppointmentTypeDurationMinutes).IsRequired();
            entity.HasIndex(x => x.TimeSlotId).IsUnique();

            entity
                .HasOne(x => x.User)
                .WithMany(x => x.Appointments)
                .HasForeignKey(x => x.UserId)
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
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<IdentityRole>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "sdfsdfsdfdfg" },
                new IdentityRole { Id = "2", Name = "Doctor", NormalizedName = "DOCTOR", ConcurrencyStamp = "sdfsdfsfdfg" },
                new IdentityRole { Id = "3", Name = "Patient", NormalizedName = "PATIENT", ConcurrencyStamp = "sdanjkdfsfdfg" }
             );
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(x => x.CityId);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.District).IsRequired();
            entity.Property(x => x.Voivodeship).IsRequired();
            entity.HasIndex(x => new { x.Name, x.District, x.Voivodeship }).IsUnique();
        });
    }
}
