using CMS.BLL.Interfaces;
using CMS.BLL.Services;
using CMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    // UC-05: register a new doctor with a temporary password
    [HttpPost]
    [Authorize(Roles = "ClinicManager")]
    public async Task<IActionResult> Create([FromBody] RegisterDoctorRequest request)
    {
        var createdDoctor = await _doctorService.CreateAsync(request.Doctor, request.TemporaryPassword);
        return Ok(createdDoctor);
    }

    // UC-06: Edit Doctor Details
    [HttpPut("{id}")]
    [Authorize(Roles = "ClinicManager")]
    public async Task<IActionResult> Update(int id, [FromBody] Doctor updatedDoctor)
    {
        var doctor = await _doctorService.UpdateAsync(id, updatedDoctor);
        return Ok(doctor);
    }

    // UC-07: DELETE Doctor
    [HttpDelete("{id}")]
    [Authorize(Roles = "ClinicManager")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _doctorService.DeleteAsync(id);
        return Ok(new { message = "Doctor deleted successfully" });
    }

    // جلب كل الأطباء
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    
    // UC-08: set doctor availability
    [HttpPost("{id}/availability")]
    [Authorize(Roles = "ClinicManager")]
    public async Task<IActionResult> SetAvailability(int id, [FromBody] List<DoctorAvailability> availabilities)
    {
        await _doctorService.SetAvailabilityAsync(id, availabilities);
        return Ok(new { message = "Schedule updated successfully" });
    }

    // UC-12: view doctor schedule 
    [HttpGet("{id}/schedule")]
    [Authorize(Roles = "Receptionist,Doctor")]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] DateTime date)
    {
        var appointments = await _doctorService.GetScheduleAsync(id, date);
        return Ok(appointments);
    }
}

// كلاس لاستقبال بيانات الدكتور + الباسورد من 
public class RegisterDoctorRequest
{
    public Doctor Doctor { get; set; } = null!;
    public string TemporaryPassword { get; set; } = "Doctor@123"; // كلمة مرور افتراضية
}