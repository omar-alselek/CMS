using CMS.Models.Entities;

namespace CMS.BLL.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetAllAsync(); // new
    Task<Appointment> BookAsync(Appointment appointment);
    Task<Appointment> ConfirmAsync(int id);
    Task<Appointment> CancelAsync(int id);
    Task<Appointment> CompleteAsync(int id);
    Task<Appointment> RescheduleAsync(int id, DateTime newDate, TimeSpan newStart, TimeSpan newEnd);
}