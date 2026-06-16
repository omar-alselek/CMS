using CMS.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ClinicManager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // UC-17: Dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _reportService.GetDashboardAsync();
        return Ok(result);
    }

    // UC-18: Patient Report
    [HttpGet("patients")]
    public async Task<IActionResult> GetPatientReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _reportService.GetPatientReportAsync(from, to);
        return Ok(result);
    }

    // UC-19: Appointment Report
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointmentReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _reportService.GetAppointmentReportAsync(from, to);
        return Ok(result);
    }
}