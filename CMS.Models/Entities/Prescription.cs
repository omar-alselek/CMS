//namespace CMS.Models.Entities;

//public class Prescription : BaseEntity
//{
//    public int AppointmentId { get; set; }
//    public Appointment Appointment { get; set; } = null!;
//    public int PatientId { get; set; }
//    public Patient Patient { get; set; } = null!;
//    public int DoctorId { get; set; }
//    public Doctor Doctor { get; set; } = null!;
//    public string Medication { get; set; } = string.Empty;
//    public string? Notes { get; set; }
//    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
//}
namespace CMS.Models.Entities;

public class Prescription : BaseEntity
{
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public string Medication { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
}