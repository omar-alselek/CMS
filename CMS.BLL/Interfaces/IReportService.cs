namespace CMS.BLL.Interfaces;

public interface IReportService
{
    Task<DashboardResult> GetDashboardAsync();
    Task<PatientReportResult> GetPatientReportAsync(DateTime? from, DateTime? to);
    Task<AppointmentReportResult> GetAppointmentReportAsync(DateTime? from, DateTime? to);
}

// UC-17: Dashboard
public class DashboardResult
{
    public int TotalPatients { get; set; }
    public int NewPatientsThisMonth { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int ScheduledCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
}

// UC-18: Patient Report
public class PatientReportResult
{
    public int TotalPatients { get; set; }
    public int NewPatients { get; set; }
    public double AverageAge { get; set; }
}

// UC-19: Appointment Report
public class AppointmentReportResult
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public double AverageAppointmentsPerDay { get; set; }
    public List<DoctorAppointmentCount> AppointmentsPerDoctor { get; set; } = new();
}

//  UC-19 و UC-20
public class DoctorAppointmentCount
{
    public string DoctorName { get; set; } = string.Empty;
    public int Count { get; set; }
}