using CMS.BLL.Interfaces;
using CMS.DAL;
using CMS.Models.Entities;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CMS.BLL.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly ClinicDbContext _context;

    public PrescriptionService(ClinicDbContext context)
    {
        _context = context;
    }

    // UC-14: Create Prescription
    public async Task<Prescription> CreateAsync(Prescription prescription)
    {
        // 1. Check appointment exists
        var appointment = await _context.Appointments.FindAsync(prescription.AppointmentId);
        if (appointment == null)
            throw new Exception("Appointment not found");

        // 2. Check appointment is Confirmed
        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new Exception("Appointment must be Confirmed before creating a prescription");

        // 3. Check no prescription already exists for this appointment
        bool prescriptionExists = await _context.Prescriptions
            .AnyAsync(p => p.AppointmentId == prescription.AppointmentId);
        if (prescriptionExists)
            throw new Exception("A prescription already exists for this appointment");

        // 4. Check medication is not empty
        //if (string.IsNullOrWhiteSpace(prescription.Medication))
          //  throw new Exception("Medication is required");

        // 5. Start Transaction — two SaveChanges inside
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            // Step A: Save the prescription
            prescription.Id = 0;
            prescription.PatientId = appointment.PatientId;
            prescription.DoctorId = appointment.DoctorId;
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Step B: Update appointment status to Completed
            appointment.Status = AppointmentStatus.Completed;
            await _context.SaveChangesAsync();

            // Both succeeded — commit
            await tx.CommitAsync();
        }
        catch
        {
            // One failed — rollback everything
            await tx.RollbackAsync();
            throw;
        }

        return prescription;
    }

    // UC-15: View Prescription by Appointment
    public async Task<Prescription> GetByAppointmentAsync(int appointmentId)
    {
        var prescription = await _context.Prescriptions
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);

        if (prescription == null)
            throw new Exception("No prescription found for this appointment");

        return prescription;
    }
}