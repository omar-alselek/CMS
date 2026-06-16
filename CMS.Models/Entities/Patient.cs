namespace CMS.Models.Entities;

public class Patient : Person
{
    public string? BloodType { get; set; }
    public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}