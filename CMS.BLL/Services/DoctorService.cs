using CMS.BLL.Interfaces;
using CMS.DAL;
using CMS.Models.Entities;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CMS.BLL.Services;

public class DoctorService : IDoctorService
{
    private readonly ClinicDbContext _context;

    public DoctorService(ClinicDbContext context)
    {
        _context = context;
    }

    // UC-05: Create a new doctor and login account
    public async Task<Doctor> CreateAsync(Doctor doctor, string temporaryPassword)
    {
        // Check required fields
        if (string.IsNullOrWhiteSpace(doctor.FullName))
            throw new Exception("Full name is required");

        if (string.IsNullOrWhiteSpace(doctor.Email))
            throw new Exception("Email is required");

        if (string.IsNullOrWhiteSpace(doctor.LicenseNumber))
            throw new Exception("License number is required");

        // Check if email already exists
        if (await _context.Doctors.AnyAsync(d => d.Email == doctor.Email))
            throw new Exception("Email already in use");

        // Check if license number already exists
        if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == doctor.LicenseNumber))
            throw new Exception("License number already exists");

        // Clear ID before saving
        //doctor.Id = 0;
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        // Create login account for the doctor
        var account = new UserAccount
        {
            PersonId = doctor.Id,

            // Use email as username
            Username = doctor.Email,

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
            Role = UserRole.Doctor,
            IsActive = true
        };

        _context.UserAccounts.Add(account);
        await _context.SaveChangesAsync();

        return doctor;
    }

    // UC-06: Update doctor information
    public async Task<Doctor> UpdateAsync(int id, Doctor updatedDoctor)
    {
        // Find doctor by ID
        var doctor = await _context.Doctors.FindAsync(id);

        if (doctor == null)
            throw new Exception("Doctor not found");

        // Check if phone number is used by another doctor
        if (await _context.Doctors.AnyAsync(d => d.Phone == updatedDoctor.Phone && d.Id != id))
            throw new Exception("Phone number already in use");

        // Check if email is used by another doctor
        if (await _context.Doctors.AnyAsync(d => d.Email == updatedDoctor.Email && d.Id != id))
            throw new Exception("Email already in use");

        // Update allowed fields
        doctor.FullName = updatedDoctor.FullName;
        doctor.Phone = updatedDoctor.Phone;
        doctor.Email = updatedDoctor.Email;
        doctor.Specialization = updatedDoctor.Specialization;

        await _context.SaveChangesAsync();

        return doctor;
    }

    // UC-07: Delete a doctor
    public async Task<bool> DeleteAsync(int id)
    {
        // Load doctor with appointments and account
        var doctor = await _context.Doctors
            .Include(d => d.Appointments)
            .Include(d => d.UserAccount)
            .FirstOrDefaultAsync(d => d.Id == id); //d هو الدكتور الحالي الذي يتم فحصه/d.Id هو رقم الدكتور/id هو الرقم الذي نبحث عنه

        if (doctor == null)
            throw new Exception("Doctor not found");

        // Check for active appointments
        bool hasActive = doctor.Appointments
            .Any(a => a.Status != AppointmentStatus.Completed &&
                      a.Status != AppointmentStatus.Cancelled);

        if (hasActive)
            throw new Exception("Cannot delete. Doctor has active appointments.");

        // Remove user account
        // احذف UserAccount أولاً قبل الدكتور
        var userAccount = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.PersonId == id);

        if (userAccount != null)
            _context.UserAccounts.Remove(userAccount);

        // Remove doctor
        _context.Doctors.Remove(doctor);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _context.Doctors.ToListAsync();
    }

    // UC-08: Set doctor availability
    public async Task SetAvailabilityAsync(int doctorId, List<DoctorAvailability> availabilities)
    {
        // Check if doctor exists
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);

        if (!doctorExists)
            throw new Exception("Doctor not found");

        // Remove old availability records //لعدم تكرار البيانات القديمة مع الجديدة
        var oldAvailabilities = await _context.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId)
            .ToListAsync();

        _context.DoctorAvailabilities.RemoveRange(oldAvailabilities);

        // Add new availability records
        foreach (var avail in availabilities)
        {
            // End time must be after start time
            if (avail.StartTime >= avail.EndTime)
                throw new Exception("End time must be after start time");

            avail.Id = 0;
            avail.DoctorId = doctorId;

            _context.DoctorAvailabilities.Add(avail);
        }

        await _context.SaveChangesAsync();
    }

    // UC-12: Get doctor's schedule for a specific day
    public async Task<IEnumerable<Appointment>> GetScheduleAsync(int doctorId, DateTime date)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                        a.AppointmentDate.Date == date.Date)
            .ToListAsync();
    }
}
/* 
 public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments.ToListAsync();
    }

             Task<IEnumerable<Appointment>> GetAllAsync();

 
 */

