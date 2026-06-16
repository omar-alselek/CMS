using CMS.BLL.Interfaces;
using CMS.DAL;
using CMS.Models.Entities;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CMS.BLL.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ClinicDbContext _context;

    public AppointmentService(ClinicDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments.ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .ToListAsync();
    }

    // ─────────────────────────────────────────
    // UC-09: Book a new appointment
    // ─────────────────────────────────────────
    public async Task<Appointment> BookAsync(Appointment appointment)
    {
        // Check if patient exists
        bool patientExists = await _context.Patients
            .AnyAsync(p => p.Id == appointment.PatientId);

        if (!patientExists)
            throw new Exception("Patient not found");

        // Check if doctor exists
        bool doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == appointment.DoctorId);

        if (!doctorExists)
            throw new Exception("Doctor not found");

        // Check if receptionist exists
        bool receptionistExists = await _context.Receptionists
            .AnyAsync(r => r.Id == appointment.ReceptionistId);

        if (!receptionistExists)
            throw new Exception("Receptionist not found");

        // Check if appointment date is not in the past
        if (appointment.AppointmentDate.Date < DateTime.UtcNow.Date)
            throw new Exception("Cannot book an appointment in the past");

        // Check if doctor works on this day
        byte dayOfWeek = (byte)appointment.AppointmentDate.DayOfWeek;

        var availability = await _context.DoctorAvailabilities
            .FirstOrDefaultAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.DayOfWeek == dayOfWeek);

        if (availability == null)
            throw new Exception("Doctor is not available on this day");

        // Check if appointment is within doctor's working hours
        if (appointment.StartTime < availability.StartTime ||
            appointment.EndTime > availability.EndTime)
            throw new Exception("Appointment is outside doctor's working hours");

        // Check if appointment duration is exactly 30 minutes
        var duration = appointment.EndTime - appointment.StartTime;

        if (duration.TotalMinutes != 30)
            throw new Exception("Appointment duration must be exactly 30 minutes");

        // Check for overlapping appointments
        bool hasOverlap = await _context.Appointments
            .AnyAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                a.Status != AppointmentStatus.Cancelled &&
                appointment.StartTime < a.EndTime &&
                appointment.EndTime > a.StartTime);

        if (hasOverlap)
            throw new Exception("This time slot is not available");

        // Save the appointment
        appointment.Id = 0;
        appointment.Status = AppointmentStatus.Scheduled;
        appointment.CreatedAt = DateTime.UtcNow;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return appointment;
    }

    // ─────────────────────────────────────────
    // UC-10: Confirm an appointment
    // ─────────────────────────────────────────
    public async Task<Appointment> ConfirmAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            throw new Exception("Appointment not found");

        // Only Scheduled or Rescheduled appointments can be confirmed
        if (appointment.Status != AppointmentStatus.Scheduled &&
            appointment.Status != AppointmentStatus.Rescheduled)
            throw new Exception("Only Scheduled or Rescheduled appointments can be confirmed");

        appointment.Status = AppointmentStatus.Confirmed;

        await _context.SaveChangesAsync();

        return appointment;
    }

    // ─────────────────────────────────────────
    // UC-11: Cancel an appointment
    // ─────────────────────────────────────────
    public async Task<Appointment> CancelAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            throw new Exception("Appointment not found");

        // Cannot cancel a completed appointment
        if (appointment.Status == AppointmentStatus.Completed)
            throw new Exception("Cannot cancel a completed appointment");

        // Prevent cancelling an already cancelled appointment
        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new Exception("Appointment is already cancelled");

        appointment.Status = AppointmentStatus.Cancelled;

        await _context.SaveChangesAsync();

        return appointment;
    }

    // ─────────────────────────────────────────
    // UC-12: Complete an appointment
    // ─────────────────────────────────────────
    public async Task<Appointment> CompleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            throw new Exception("Appointment not found");

        // Only Confirmed appointments can be completed
        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new Exception("Only confirmed appointments can be completed");

        appointment.Status = AppointmentStatus.Completed;

        await _context.SaveChangesAsync();

        return appointment;
    }

    // ─────────────────────────────────────────
    // UC-13: Reschedule an appointment
    // ─────────────────────────────────────────
    public async Task<Appointment> RescheduleAsync(
    int id,
    DateTime newDate,
    TimeSpan newStart,
    TimeSpan newEnd)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            throw new Exception("Appointment not found");

        if (appointment.Status != AppointmentStatus.Scheduled &&
            appointment.Status != AppointmentStatus.Confirmed)
            throw new Exception("Only Scheduled or Confirmed appointments can be rescheduled");

        
         if (newDate.Date < DateTime.UtcNow.Date)
             throw new Exception("Cannot reschedule to a past date");

        byte dayOfWeek = (byte)newDate.DayOfWeek;

        var availability = await _context.DoctorAvailabilities
            .FirstOrDefaultAsync(a =>
                a.DoctorId == appointment.DoctorId &&
                a.DayOfWeek == dayOfWeek);

        if (availability == null)
            throw new Exception("Doctor is not available on this day");

        if (newStart < availability.StartTime || newEnd > availability.EndTime)
            throw new Exception("Appointment is outside doctor's working hours");

        var duration = newEnd - newStart;
        if (duration.TotalMinutes != 30)
            throw new Exception("Appointment duration must be exactly 30 minutes");

        bool hasOverlap = await _context.Appointments
            .AnyAsync(a =>
                a.Id != id &&
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate.Date == newDate.Date &&
                a.Status != AppointmentStatus.Cancelled &&
                newStart < a.EndTime &&
                newEnd > a.StartTime);

        if (hasOverlap)
            throw new Exception("This time slot is not available");

        appointment.AppointmentDate = newDate;
        appointment.StartTime = newStart;
        appointment.EndTime = newEnd;
        appointment.Status = AppointmentStatus.Rescheduled;

        await _context.SaveChangesAsync();
        return appointment;
    }
}