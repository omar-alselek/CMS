using CMS.BLL.Interfaces;
using CMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    // UC-14: Create prescription
    [HttpPost]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Create([FromBody] Prescription prescription)
    {
        var result = await _prescriptionService.CreateAsync(prescription);
        return Ok(result);
    }

    // UC-15: View prescription by appointment
    [HttpGet("appointment/{appointmentId}")]
    [Authorize(Roles = "Doctor,Receptionist")]
    public async Task<IActionResult> GetByAppointment(int appointmentId)
    {
        var result = await _prescriptionService.GetByAppointmentAsync(appointmentId);
        return Ok(result);
    }
}