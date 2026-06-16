using CMS.BLL.Interfaces;
using CMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // جلب مواعيد دكتور معين
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(int doctorId)
    {
        var appointments = await _appointmentService.GetByDoctorAsync(doctorId);
        return Ok(appointments);
    }

    [HttpGet] // new endpoint to get all appointments
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _appointmentService.GetAllAsync();
        return Ok(appointments);
    }

    // UC-09
    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Book([FromBody] Appointment appointment)
    {
        var result = await _appointmentService.BookAsync(appointment);
        return Ok(result);
    }

    // UC-10
    [HttpPut("{id}/confirm")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await _appointmentService.ConfirmAsync(id);
        return Ok(result);
    }

    // UC-11
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _appointmentService.CancelAsync(id);
        return Ok(result);
    }

    // UC: Complete
    [HttpPut("{id}/complete")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Complete(int id)
    {
        var result = await _appointmentService.CompleteAsync(id);
        return Ok(result);
    }

    // UC-13
    [HttpPut("{id}/reschedule")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequest request)
    {
        var result = await _appointmentService.RescheduleAsync(
            id,
            request.NewDate,
            request.NewStart,
            request.NewEnd);
        return Ok(result);
    }
}

  // receiving Reschedule data
public class RescheduleRequest
{
    public DateTime NewDate { get; set; }
    public TimeSpan NewStart { get; set; }
    public TimeSpan NewEnd { get; set; }
}