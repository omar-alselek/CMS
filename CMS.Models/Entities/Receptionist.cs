namespace CMS.Models.Entities;

public class Receptionist : Person
{
    public TimeSpan? ShiftStart { get; set; }
    public TimeSpan? ShiftEnd { get; set; }
    public UserAccount? UserAccount { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}