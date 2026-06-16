using CMS.Models.Entities;

namespace CMS.BLL.Interfaces;

public interface IPrescriptionService
{
    Task<Prescription> CreateAsync(Prescription prescription);
    Task<Prescription> GetByAppointmentAsync(int appointmentId);
}