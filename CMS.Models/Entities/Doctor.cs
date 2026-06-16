namespace CMS.Models.Entities;

public class Doctor : Person
{
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public UserAccount? UserAccount { get; set; }
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}