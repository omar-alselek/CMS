using CMS.Models.Entities;

namespace CMS.BLL.Interfaces;

public interface IDoctorService
{
    // UC-05: register a new doctor with a temporary password
    Task<Doctor> CreateAsync(Doctor doctor, string temporaryPassword);

    // UC-06: تعديل بيانات الدكتور
    Task<Doctor> UpdateAsync(int id, Doctor updatedDoctor);

    // UC-07: حذف دكتور
    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<Doctor>> GetAllAsync();

    // UC-08: تحديد أوقات تواجد الدكتور
    Task SetAvailabilityAsync(int doctorId, List<DoctorAvailability> availabilities);

    // UC-12: عرض جدول الدكتور 
    Task<IEnumerable<Appointment>> GetScheduleAsync(int doctorId, DateTime date);
}