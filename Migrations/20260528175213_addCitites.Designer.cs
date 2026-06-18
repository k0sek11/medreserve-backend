
using System;
using Medreserve.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Medreserve.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260528175213_addCitites")]
    partial class addCitites
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Medreserve.Features.Appointment.Appointment", b =>
                {
                    b.Property<int>("AppointmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("appointment_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AppointmentId"));

                    b.Property<int>("AppointmentTypeDurationMinutes")
                        .HasColumnType("integer")
                        .HasColumnName("appointment_type_duration_minutes");

                    b.Property<int?>("AppointmentTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("appointment_type_id");

                    b.Property<string>("CancellationReason")
                        .HasColumnType("text")
                        .HasColumnName("cancellation_reason");

                    b.Property<DateTime?>("CancelledAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("cancelled_at");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("completed_at");

                    b.Property<DateTime?>("ConfirmedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("confirmed_at");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<int>("DoctorId")
                        .HasColumnType("integer")
                        .HasColumnName("doctor_id");

                    b.Property<string>("DoctorNotes")
                        .HasColumnType("text")
                        .HasColumnName("doctor_notes");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<int>("TimeSlotId")
                        .HasColumnType("integer")
                        .HasColumnName("time_slot_id");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("AppointmentId")
                        .HasName("pk_appointments");

                    b.HasIndex("AppointmentTypeId")
                        .HasDatabaseName("ix_appointments_appointment_type_id");

                    b.HasIndex("DoctorId")
                        .HasDatabaseName("ix_appointments_doctor_id");

                    b.HasIndex("TimeSlotId")
                        .IsUnique()
                        .HasDatabaseName("ix_appointments_time_slot_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_appointments_user_id");

                    b.ToTable("appointments", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.AppointmentType.AppointmentType", b =>
                {
                    b.Property<int>("AppointmentTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("appointment_type_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AppointmentTypeId"));

                    b.Property<decimal>("BasePrice")
                        .HasColumnType("numeric")
                        .HasColumnName("base_price");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<int>("DurationMinutes")
                        .HasColumnType("integer")
                        .HasColumnName("duration_minutes");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("AppointmentTypeId")
                        .HasName("pk_appointment_types");

                    b.ToTable("appointment_types", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Clinic.Clinic", b =>
                {
                    b.Property<int>("ClinicId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("clinic_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ClinicId"));

                    b.Property<int>("CityId")
                        .HasColumnType("integer")
                        .HasColumnName("city_id");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Email")
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<string>("MapLocation")
                        .HasColumnType("text")
                        .HasColumnName("map_location");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("OpeningHours")
                        .HasColumnType("text")
                        .HasColumnName("opening_hours");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.Property<string>("StreetAddress")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("street_address");

                    b.HasKey("ClinicId")
                        .HasName("pk_clinics");

                    b.HasIndex("CityId")
                        .HasDatabaseName("ix_clinics_city_id");

                    b.ToTable("clinics", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.ClinicDoctor", b =>
                {
                    b.Property<int>("ClinicId")
                        .HasColumnType("integer")
                        .HasColumnName("clinic_id");

                    b.Property<int>("DoctorId")
                        .HasColumnType("integer")
                        .HasColumnName("doctor_id");

                    b.Property<bool>("IsOwner")
                        .HasColumnType("boolean")
                        .HasColumnName("is_owner");

                    b.HasKey("ClinicId", "DoctorId")
                        .HasName("pk_clinic_doctors");

                    b.HasIndex("DoctorId")
                        .HasDatabaseName("ix_clinic_doctors_doctor_id");

                    b.ToTable("clinic_doctors", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.Doctor", b =>
                {
                    b.Property<int>("DoctorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("doctor_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DoctorId"));

                    b.Property<string>("Bio")
                        .HasColumnType("text")
                        .HasColumnName("bio");

                    b.Property<string>("LicenseNumber")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("license_number");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("DoctorId")
                        .HasName("pk_doctors");

                    b.HasIndex("LicenseNumber")
                        .IsUnique()
                        .HasDatabaseName("ix_doctors_license_number");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasDatabaseName("ix_doctors_user_id");

                    b.ToTable("doctors", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.DoctorAppointmentType", b =>
                {
                    b.Property<int>("DoctorId")
                        .HasColumnType("integer")
                        .HasColumnName("doctor_id");

                    b.Property<int>("AppointmentTypeId")
                        .HasColumnType("integer")
                        .HasColumnName("appointment_type_id");

                    b.HasKey("DoctorId", "AppointmentTypeId")
                        .HasName("pk_doctor_appointment_types");

                    b.HasIndex("AppointmentTypeId")
                        .HasDatabaseName("ix_doctor_appointment_types_appointment_type_id");

                    b.ToTable("doctor_appointment_types", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.DoctorSchedule", b =>
                {
                    b.Property<int>("ScheduleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("schedule_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ScheduleId"));

                    b.Property<int?>("ClinicId")
                        .HasColumnType("integer")
                        .HasColumnName("clinic_id");

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("integer")
                        .HasColumnName("day_of_week");

                    b.Property<int>("DoctorId")
                        .HasColumnType("integer")
                        .HasColumnName("doctor_id");

                    b.Property<string>("EndTime")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("end_time");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<string>("StartTime")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("start_time");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("valid_from");

                    b.Property<DateTime?>("ValidTo")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("valid_to");

                    b.HasKey("ScheduleId")
                        .HasName("pk_doctor_schedules");

                    b.HasIndex("ClinicId")
                        .HasDatabaseName("ix_doctor_schedules_clinic_id");

                    b.HasIndex("DoctorId")
                        .HasDatabaseName("ix_doctor_schedules_doctor_id");

                    b.ToTable("doctor_schedules", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.DoctorSpecialization", b =>
                {
                    b.Property<int>("DoctorId")
                        .HasColumnType("integer")
                        .HasColumnName("doctor_id");

                    b.Property<int>("SpecializationId")
                        .HasColumnType("integer")
                        .HasColumnName("specialization_id");

                    b.HasKey("DoctorId", "SpecializationId")
                        .HasName("pk_doctor_specializations");

                    b.HasIndex("SpecializationId")
                        .HasDatabaseName("ix_doctor_specializations_specialization_id");

                    b.ToTable("doctor_specializations", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Geography.City", b =>
                {
                    b.Property<int>("CityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("city_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CityId"));

                    b.Property<string>("District")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("district");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Voivodeship")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("voivodeship");

                    b.HasKey("CityId")
                        .HasName("pk_cities");

                    b.HasIndex("Name", "District", "Voivodeship")
                        .IsUnique()
                        .HasDatabaseName("ix_cities_name_district_voivodeship");

                    b.ToTable("cities", (string)null);

                    b.HasData(
                        new
                        {
                            CityId = 1,
                            District = "Śródmieście",
                            Name = "Warszawa",
                            Voivodeship = "Mazowieckie"
                        },
                        new
                        {
                            CityId = 2,
                            District = "Stare Miasto",
                            Name = "Kraków",
                            Voivodeship = "Małopolskie"
                        },
                        new
                        {
                            CityId = 3,
                            District = "Śródmieście",
                            Name = "Łódź",
                            Voivodeship = "Łódzkie"
                        },
                        new
                        {
                            CityId = 4,
                            District = "Stare Miasto",
                            Name = "Wrocław",
                            Voivodeship = "Dolnośląskie"
                        },
                        new
                        {
                            CityId = 5,
                            District = "Stare Miasto",
                            Name = "Poznań",
                            Voivodeship = "Wielkopolskie"
                        });
                });

            modelBuilder.Entity("Medreserve.Features.Notification.Notification", b =>
                {
                    b.Property<int>("NotificationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("notification_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("NotificationId"));

                    b.Property<int?>("AppointmentId")
                        .HasColumnType("integer")
                        .HasColumnName("appointment_id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("content");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("FailureReason")
                        .HasColumnType("text")
                        .HasColumnName("failure_reason");

                    b.Property<DateTime?>("SentAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("sent_at");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("subject");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("NotificationId")
                        .HasName("pk_notifications");

                    b.HasIndex("AppointmentId")
                        .HasDatabaseName("ix_notifications_appointment_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_notifications_user_id");

                    b.ToTable("notifications", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Payment.OfflinePaymentApproval", b =>
                {
                    b.Property<int>("ApprovalId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("approval_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ApprovalId"));

                    b.Property<string>("ApprovedByUserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("approved_by_user_id");

                    b.Property<string>("Comment")
                        .HasColumnType("text")
                        .HasColumnName("comment");

                    b.Property<string>("Decision")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("decision");

                    b.Property<DateTime>("DecisionDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("decision_date");

                    b.Property<int>("PaymentId")
                        .HasColumnType("integer")
                        .HasColumnName("payment_id");

                    b.HasKey("ApprovalId")
                        .HasName("pk_offline_payment_approvals");

                    b.HasIndex("ApprovedByUserId")
                        .HasDatabaseName("ix_offline_payment_approvals_approved_by_user_id");

                    b.HasIndex("PaymentId")
                        .IsUnique()
                        .HasDatabaseName("ix_offline_payment_approvals_payment_id");

                    b.ToTable("offline_payment_approvals", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Payment.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("payment_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("PaymentId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric")
                        .HasColumnName("amount");

                    b.Property<int>("AppointmentId")
                        .HasColumnType("integer")
                        .HasColumnName("appointment_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("currency");

                    b.Property<string>("FailureReason")
                        .HasColumnType("text")
                        .HasColumnName("failure_reason");

                    b.Property<string>("Method")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("method");

                    b.Property<DateTime?>("PaidAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("paid_at");

                    b.Property<string>("Provider")
                        .HasColumnType("text")
                        .HasColumnName("provider");

                    b.Property<string>("ProviderTransactionId")
                        .HasColumnType("text")
                        .HasColumnName("provider_transaction_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("PaymentId")
                        .HasName("pk_payments");

                    b.HasIndex("AppointmentId")
                        .HasDatabaseName("ix_payments_appointment_id");

                    b.ToTable("payments", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Specialization.Specialization", b =>
                {
                    b.Property<int>("SpecializationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("specialization_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SpecializationId"));

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.HasKey("SpecializationId")
                        .HasName("pk_specializations");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_specializations_name");

                    b.ToTable("specializations", (string)null);

                    b.HasData(
                        new
                        {
                            SpecializationId = 1,
                            Name = "Alergolog"
                        },
                        new
                        {
                            SpecializationId = 2,
                            Name = "Anestezjolog"
                        },
                        new
                        {
                            SpecializationId = 3,
                            Name = "Chirurg ogólny"
                        },
                        new
                        {
                            SpecializationId = 4,
                            Name = "Internista"
                        },
                        new
                        {
                            SpecializationId = 5,
                            Name = "Dermatolog"
                        },
                        new
                        {
                            SpecializationId = 6,
                            Name = "Diabetolog"
                        },
                        new
                        {
                            SpecializationId = 7,
                            Name = "Endokrynolog"
                        },
                        new
                        {
                            SpecializationId = 8,
                            Name = "Gastroenterolog"
                        },
                        new
                        {
                            SpecializationId = 9,
                            Name = "Ginekolog"
                        },
                        new
                        {
                            SpecializationId = 10,
                            Name = "Kardiolog"
                        },
                        new
                        {
                            SpecializationId = 11,
                            Name = "Lekarz medycyny pracy"
                        },
                        new
                        {
                            SpecializationId = 12,
                            Name = "Lekarz medycyny rodzinnej"
                        },
                        new
                        {
                            SpecializationId = 13,
                            Name = "Neurolog"
                        },
                        new
                        {
                            SpecializationId = 14,
                            Name = "Okulista"
                        },
                        new
                        {
                            SpecializationId = 15,
                            Name = "Onkolog"
                        },
                        new
                        {
                            SpecializationId = 16,
                            Name = "Ortopeda"
                        },
                        new
                        {
                            SpecializationId = 17,
                            Name = "Pediatra"
                        },
                        new
                        {
                            SpecializationId = 18,
                            Name = "Psychiatra"
                        },
                        new
                        {
                            SpecializationId = 19,
                            Name = "Pulmonolog"
                        },
                        new
                        {
                            SpecializationId = 20,
                            Name = "Urolog"
                        });
                });

            modelBuilder.Entity("Medreserve.Features.Users.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer")
                        .HasColumnName("access_failed_count");

                    b.Property<DateOnly?>("BirthDate")
                        .HasColumnType("date")
                        .HasColumnName("birth_date");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasColumnName("concurrency_stamp");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("email");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("email_confirmed");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("first_name");

                    b.Property<string>("Gender")
                        .HasColumnType("text")
                        .HasColumnName("gender");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("last_name");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("lockout_enabled");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lockout_end");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_email");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_user_name");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text")
                        .HasColumnName("password_hash");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean")
                        .HasColumnName("phone_number_confirmed");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text")
                        .HasColumnName("security_stamp");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("two_factor_enabled");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("pk_asp_net_users");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text")
                        .HasColumnName("concurrency_stamp");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("normalized_name");

                    b.HasKey("Id")
                        .HasName("pk_asp_net_roles");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_asp_net_roles_name");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "1",
                            ConcurrencyStamp = "sdfsdfsdfdfg",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "2",
                            ConcurrencyStamp = "sdfsdfsfdfg",
                            Name = "Doctor",
                            NormalizedName = "DOCTOR"
                        },
                        new
                        {
                            Id = "3",
                            ConcurrencyStamp = "sdanjkdfsfdfg",
                            Name = "Patient",
                            NormalizedName = "PATIENT"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text")
                        .HasColumnName("claim_value");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_asp_net_role_claims");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_asp_net_role_claims_role_id");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text")
                        .HasColumnName("claim_value");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_asp_net_user_claims");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_asp_net_user_claims_user_id");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text")
                        .HasColumnName("login_provider");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text")
                        .HasColumnName("provider_key");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text")
                        .HasColumnName("provider_display_name");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("LoginProvider", "ProviderKey")
                        .HasName("pk_asp_net_user_logins");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_asp_net_user_logins_user_id");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.Property<string>("RoleId")
                        .HasColumnType("text")
                        .HasColumnName("role_id");

                    b.HasKey("UserId", "RoleId")
                        .HasName("pk_asp_net_user_roles");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_asp_net_user_roles_role_id");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text")
                        .HasColumnName("login_provider");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Value")
                        .HasColumnType("text")
                        .HasColumnName("value");

                    b.HasKey("UserId", "LoginProvider", "Name")
                        .HasName("pk_asp_net_user_tokens");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Medreserve.Features.Appointment.Appointment", b =>
                {
                    b.HasOne("Medreserve.Features.AppointmentType.AppointmentType", "AppointmentType")
                        .WithMany("Appointments")
                        .HasForeignKey("AppointmentTypeId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_appointments_appointment_types_appointment_type_id");

                    b.HasOne("Medreserve.Features.Doctor.Doctor", "Doctor")
                        .WithMany("Appointments")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_appointments_doctors_doctor_id");

                    b.HasOne("Medreserve.Features.Users.User", "User")
                        .WithMany("Appointments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_appointments_users_user_id");

                    b.Navigation("AppointmentType");

                    b.Navigation("Doctor");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Medreserve.Features.Clinic.Clinic", b =>
                {
                    b.HasOne("Medreserve.Features.Geography.City", "City")
                        .WithMany("Clinics")
                        .HasForeignKey("CityId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_clinics_cities_city_id");

                    b.Navigation("City");
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.ClinicDoctor", b =>
                {
                    b.HasOne("Medreserve.Features.Clinic.Clinic", "Clinic")
                        .WithMany("ClinicDoctors")
                        .HasForeignKey("ClinicId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_clinic_doctors_clinics_clinic_id");

                    b.HasOne("Medreserve.Features.Doctor.Doctor", "Doctor")
                        .WithMany("ClinicDoctors")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_clinic_doctors_doctors_doctor_id");

                    b.Navigation("Clinic");

                    b.Navigation("Doctor");
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.Doctor", b =>
                {
                    b.HasOne("Medreserve.Features.Users.User", "User")
                        .WithOne("DoctorProfile")
                        .HasForeignKey("Medreserve.Features.Doctor.Doctor", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_doctors_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.DoctorAppointmentType", b =>
                {
                    b.HasOne("Medreserve.Features.AppointmentType.AppointmentType", "AppointmentType")
                        .WithMany("DoctorAppointmentTypes")
                        .HasForeignKey("AppointmentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_doctor_appointment_types_appointment_types_appointment_type");

                    b.HasOne("Medreserve.Features.Doctor.Doctor", "Doctor")
                        .WithMany("DoctorAppointmentTypes")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_doctor_appointment_types_doctors_doctor_id");

                    b.Navigation("AppointmentType");

                    b.Navigation("Doctor");
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.DoctorSchedule", b =>
                {
                    b.HasOne("Medreserve.Features.Clinic.Clinic", "Clinic")
                        .WithMany()
                        .HasForeignKey("ClinicId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_doctor_schedules_clinics_clinic_id");

                    b.HasOne("Medreserve.Features.Doctor.Doctor", "Doctor")
                        .WithMany("DoctorSchedules")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_doctor_schedules_doctors_doctor_id");

                    b.Navigation("Clinic");

                    b.Navigation("Doctor");
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.DoctorSpecialization", b =>
                {
                    b.HasOne("Medreserve.Features.Doctor.Doctor", "Doctor")
                        .WithMany("DoctorSpecializations")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_doctor_specializations_doctors_doctor_id");

                    b.HasOne("Medreserve.Features.Specialization.Specialization", "Specialization")
                        .WithMany("DoctorSpecializations")
                        .HasForeignKey("SpecializationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_doctor_specializations_specializations_specialization_id");

                    b.Navigation("Doctor");

                    b.Navigation("Specialization");
                });

            modelBuilder.Entity("Medreserve.Features.Notification.Notification", b =>
                {
                    b.HasOne("Medreserve.Features.Appointment.Appointment", "Appointment")
                        .WithMany("Notifications")
                        .HasForeignKey("AppointmentId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_notifications_appointments_appointment_id");

                    b.HasOne("Medreserve.Features.Users.User", "User")
                        .WithMany("Notifications")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_notifications_users_user_id");

                    b.Navigation("Appointment");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Medreserve.Features.Payment.OfflinePaymentApproval", b =>
                {
                    b.HasOne("Medreserve.Features.Users.User", "ApprovedByUser")
                        .WithMany("OfflinePaymentApprovals")
                        .HasForeignKey("ApprovedByUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_offline_payment_approvals_users_approved_by_user_id");

                    b.HasOne("Medreserve.Features.Payment.Payment", "Payment")
                        .WithOne("OfflinePaymentApproval")
                        .HasForeignKey("Medreserve.Features.Payment.OfflinePaymentApproval", "PaymentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_offline_payment_approvals_payments_payment_id");

                    b.Navigation("ApprovedByUser");

                    b.Navigation("Payment");
                });

            modelBuilder.Entity("Medreserve.Features.Payment.Payment", b =>
                {
                    b.HasOne("Medreserve.Features.Appointment.Appointment", "Appointment")
                        .WithMany("Payments")
                        .HasForeignKey("AppointmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_payments_appointments_appointment_id");

                    b.Navigation("Appointment");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_asp_net_role_claims_asp_net_roles_role_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Medreserve.Features.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_asp_net_user_claims_asp_net_users_user_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Medreserve.Features.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_asp_net_user_logins_asp_net_users_user_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_asp_net_user_roles_asp_net_roles_role_id");

                    b.HasOne("Medreserve.Features.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_asp_net_user_roles_asp_net_users_user_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Medreserve.Features.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_asp_net_user_tokens_asp_net_users_user_id");
                });

            modelBuilder.Entity("Medreserve.Features.Appointment.Appointment", b =>
                {
                    b.Navigation("Notifications");

                    b.Navigation("Payments");
                });

            modelBuilder.Entity("Medreserve.Features.AppointmentType.AppointmentType", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("DoctorAppointmentTypes");
                });

            modelBuilder.Entity("Medreserve.Features.Clinic.Clinic", b =>
                {
                    b.Navigation("ClinicDoctors");
                });

            modelBuilder.Entity("Medreserve.Features.Doctor.Doctor", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("ClinicDoctors");

                    b.Navigation("DoctorAppointmentTypes");

                    b.Navigation("DoctorSchedules");

                    b.Navigation("DoctorSpecializations");
                });

            modelBuilder.Entity("Medreserve.Features.Geography.City", b =>
                {
                    b.Navigation("Clinics");
                });

            modelBuilder.Entity("Medreserve.Features.Payment.Payment", b =>
                {
                    b.Navigation("OfflinePaymentApproval");
                });

            modelBuilder.Entity("Medreserve.Features.Specialization.Specialization", b =>
                {
                    b.Navigation("DoctorSpecializations");
                });

            modelBuilder.Entity("Medreserve.Features.Users.User", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("DoctorProfile");

                    b.Navigation("Notifications");

                    b.Navigation("OfflinePaymentApprovals");
                });
#pragma warning restore 612, 618
        }
    }
}
