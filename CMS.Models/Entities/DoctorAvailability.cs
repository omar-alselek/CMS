namespace CMS.Models.Entities;

public class DoctorAvailability : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public byte DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}