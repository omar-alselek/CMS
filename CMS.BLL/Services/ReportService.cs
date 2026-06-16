using CMS.BLL.Interfaces;
using CMS.DAL;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CMS.BLL.Services;

public class ReportService : IReportService
{
    private readonly ClinicDbContext _context;

    public ReportService(ClinicDbContext context)
    {
        _context = context;
    }

    // UC-17: Dashboard
    public async Task<DashboardResult> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        int totalPatients = await _context.Patients.CountAsync();

        int newPatientsThisMonth = await _context.Patients
            .CountAsync(p => p.RegisteredDate >= startOfMonth);

        int totalDoctors = await _context.Doctors.CountAsync();

        int totalAppointments = await _context.Appointments.CountAsync();

        int scheduledCount = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Scheduled);

        int confirmedCount = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Confirmed);

        int completedCount = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Completed);

        int cancelledCount = await _context.Appointments
            .CountAsync(a => a.Status == AppointmentStatus.Cancelled);

        return new DashboardResult
        {
            TotalPatients = totalPatients,
            NewPatientsThisMonth = newPatientsThisMonth,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            ScheduledCount = scheduledCount,
            ConfirmedCount = confirmedCount,
            CompletedCount = completedCount,
            CancelledCount = cancelledCount
        };
    }

    // UC-18: Patient Report
    public async Task<PatientReportResult> GetPatientReportAsync(DateTime? from, DateTime? to)
    {
        // If no date is specified — we use all data
        var fromDate = from ?? DateTime.MinValue;
        var toDate = to ?? DateTime.MaxValue;

        int totalPatients = await _context.Patients.CountAsync();

        int newPatients = await _context.Patients
            .CountAsync(p => p.RegisteredDate >= fromDate && p.RegisteredDate <= toDate);

        // Calculating average age
        var birthDates = await _context.Patients
            .Where(p => p.DateOfBirth != null)
            .Select(p => p.DateOfBirth)
            .ToListAsync();

        double averageAge = 0;
        if (birthDates.Count > 0)
        {
            var today = DateTime.Today;
            averageAge = birthDates
                .Where(d => d.HasValue)
                .Average(d => (today - d!.Value).TotalDays / 365.25);
        }

        return new PatientReportResult
        {
            TotalPatients = totalPatients,
            NewPatients = newPatients,
            AverageAge = Math.Round(averageAge, 1)
        };
    }

    // UC-19: Appointment Report
    public async Task<AppointmentReportResult> GetAppointmentReportAsync(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.MinValue;
        var toDate = to ?? DateTime.MaxValue;

        // جيب المواعيد ضمن الفترة
        var appointments = await _context.Appointments
            .Where(a => a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate)
            .ToListAsync();

        int total = appointments.Count;
        int completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        int cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);

        // average appointments per day
        double avgPerDay = 0;
        if (total > 0)
        {
            var days = (toDate - fromDate).TotalDays;
            avgPerDay = days > 0 ? Math.Round(total / days, 1) : total;
        }

        // appointements all doctors
        var perDoctor = await _context.Appointments
            .Where(a => a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate)
            .Include(a => a.Doctor)
            .GroupBy(a => a.Doctor.FullName)
            .Select(g => new DoctorAppointmentCount
            {
                DoctorName = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        return new AppointmentReportResult
        {
            TotalAppointments = total,
            CompletedAppointments = completed,
            CancelledAppointments = cancelled,
            AverageAppointmentsPerDay = avgPerDay,
            AppointmentsPerDoctor = perDoctor
        };
    }
}