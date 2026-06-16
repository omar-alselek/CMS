using CMS.Models.Entities;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CMS.DAL;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Receptionist> Receptionists { get; set; }
    public DbSet<ClinicManager> ClinicManagers { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TPH Strategy - كل Person types في جدول واحد
        modelBuilder.Entity<Person>()
            .HasDiscriminator<string>("PersonType")
            .HasValue<Patient>("Patient")
            .HasValue<Doctor>("Doctor")
            .HasValue<Receptionist>("Receptionist")
            .HasValue<ClinicManager>("ClinicManager");

        // Person constraints
        modelBuilder.Entity<Person>()
            .Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Person>()
            .Property(p => p.Phone)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<Person>()
            .HasIndex(p => p.Phone)
            .IsUnique();

        modelBuilder.Entity<Person>()
            .Property(p => p.Email)
            .HasMaxLength(100);

        modelBuilder.Entity<Person>()
            .Property(p => p.Address)
            .HasMaxLength(200);

        // Doctor constraints
        modelBuilder.Entity<Doctor>()
            .Property(d => d.Specialization)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Doctor>()
            .Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Doctor>()
            .HasIndex(d => d.LicenseNumber)
            .IsUnique();

        // UserAccount constraints
        modelBuilder.Entity<UserAccount>()
            .Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<UserAccount>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<UserAccount>()
            .HasIndex(u => u.PersonId)
            .IsUnique();

        modelBuilder.Entity<UserAccount>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<UserAccount>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        modelBuilder.Entity<UserAccount>()
            .Property(u => u.IsActive)
            .HasDefaultValue(true);

        // UserAccount → Person relationship
        modelBuilder.Entity<UserAccount>()
            .HasOne(u => u.Person)
            .WithOne()
            .HasForeignKey<UserAccount>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // DoctorAvailability constraints
        modelBuilder.Entity<DoctorAvailability>()
            .HasIndex(d => new { d.DoctorId, d.DayOfWeek, d.StartTime })
            .IsUnique();

        modelBuilder.Entity<DoctorAvailability>()
            .HasOne(d => d.Doctor)
            .WithMany(d => d.Availabilities)
            .HasForeignKey(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Appointment constraints - FR-14
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.StartTime })
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AppointmentStatus.Scheduled);

        modelBuilder.Entity<Appointment>()
            .Property(a => a.Notes)
            .HasMaxLength(500);

        // Appointment relationships
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Receptionist)
            .WithMany(r => r.Appointments)
            .HasForeignKey(a => a.ReceptionistId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prescription constraints
        modelBuilder.Entity<Prescription>()
            .HasIndex(p => p.AppointmentId)
            .IsUnique();

        modelBuilder.Entity<Prescription>()
            .Property(p => p.Medication)
            .IsRequired()
            .HasMaxLength(500);

        modelBuilder.Entity<Prescription>()
            .Property(p => p.Notes)
            .HasMaxLength(500);

        // Prescription relationships
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Appointment)
            .WithOne(a => a.Prescription)
            .HasForeignKey<Prescription>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Patient)
            .WithMany(p => p.Prescriptions)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Doctor)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}