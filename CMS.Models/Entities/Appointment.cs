using CMS.Models.Enums;
namespace CMS.Models.Entities;
public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public int ReceptionistId { get; set; }
    public Receptionist? Receptionist { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }
    public Prescription? Prescription { get; set; }
}