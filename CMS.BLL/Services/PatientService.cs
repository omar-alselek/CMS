/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.BLL.Services
{
    internal class PatientService
    {
    }
}
*/
using CMS.BLL.Interfaces;
using CMS.DAL;
using CMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.BLL.Services;

public class PatientService : IPatientService
{
    private readonly ClinicDbContext _context;

    public PatientService(ClinicDbContext context)
    {
        _context = context;
    }

    // UC-01: input patient data new patient 
    public async Task<Patient> CreateAsync(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
            throw new Exception("Full name is required");

        if (string.IsNullOrWhiteSpace(patient.Phone))
            throw new Exception("Phone is required");

        if (patient.DateOfBirth > DateTime.UtcNow)
            throw new Exception("Invalid birth date");

        var exists = await _context.Patients.AnyAsync(p => p.Phone == patient.Phone);
        if (exists)
            throw new Exception("Phone already exists");

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    // UC-02: edit patient data
    public async Task<Patient> UpdateAsync(int id, Patient updatedPatient)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
            throw new Exception("Patient not found");

        // نحدث بس الحقول اللي مسموح نغيرها
        patient.FullName = updatedPatient.FullName;
        patient.Phone = updatedPatient.Phone;
        patient.Email = updatedPatient.Email;
        patient.Address = updatedPatient.Address;
        patient.BloodType = updatedPatient.BloodType;
        patient.DateOfBirth = updatedPatient.DateOfBirth;

        await _context.SaveChangesAsync();
        return patient;
    }

    // UC-03: search for patients by name or phone
    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await _context.Patients.ToListAsync();

        return await _context.Patients
            .Where(p => p.FullName.Contains(searchTerm) || p.Phone.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _context.Patients.ToListAsync();
    }

    // UC-04: view patient history (appointments, prescriptions)
    //public async Task<Patient> GetHistoryAsync(int id)
    //{
    //    var patient = await _context.Patients
    //        .Include(p => p.Appointments)
    //        .ThenInclude(a => a.Doctor)
    //        .Include(p => p.Prescriptions)
    //        .FirstOrDefaultAsync(p => p.Id == id);
    //    if (patient == null)
    //        throw new Exception("Patient not found");

    //    return patient;
    //}
    public async Task<Patient> GetHistoryAsync(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Doctor)
            .Include(p => p.Prescriptions)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
            throw new Exception("Patient not found");

        // قطع العلاقات الدائرية يدوياً
        foreach (var appointment in patient.Appointments)
        {
            if (appointment.Doctor != null)
            {
                appointment.Doctor.Appointments = null!;
                appointment.Doctor.Prescriptions = null!;
            }
            appointment.Receptionist = null;
        }

        foreach (var prescription in patient.Prescriptions)
        {
            prescription.Appointment = null;
            prescription.Doctor = null;
        }

        return patient;
    }
}