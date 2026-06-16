using CMS.BLL.Interfaces;
using CMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // UC-01
    [HttpPost]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Create(Patient patient)
    {
        var createdPatient = await _patientService.CreateAsync(patient);
        return Ok(createdPatient);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _patientService.GetAllAsync();
        return Ok(patients);
    }
    // UC-02
    [HttpPut("{id}")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> Update(int id, Patient updatedPatient)
    {
        var patient = await _patientService.UpdateAsync(id, updatedPatient);
        return Ok(patient);
    }

    // UC-03
    [HttpGet("search")]
    [Authorize(Roles = "Receptionist,Doctor,ClinicManager")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _patientService.SearchAsync(q);
        return Ok(results);
    }

    // UC-04
    [HttpGet("{id}/history")]
    [Authorize(Roles = "Doctor,Receptionist")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var patient = await _patientService.GetHistoryAsync(id);
        return Ok(patient);
    }

}