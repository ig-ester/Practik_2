using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Praktik.Domain.Entities;
using Praktik.Infrastructure.Data;

namespace Praktik.Controllers;

[Route("api/prescriptions")]
[ApiController]
public class PrescriptionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PrescriptionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PrescriptionDto dto)
    {
        var prescription = new Prescription
        {
            Number = dto.Number,
            IssueDate = dto.IssueDate,
            DoctorName = dto.DoctorName,
            ClinicName = dto.ClinicName,
            PatientName = dto.PatientName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return Ok(new { id = prescription.Id, number = prescription.Number });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null)
            return NotFound();

        return Ok(new
        {
            id = prescription.Id,
            number = prescription.Number,
            issueDate = prescription.IssueDate,
            doctorName = prescription.DoctorName,
            clinicName = prescription.ClinicName,
            patientName = prescription.PatientName
        });
    }
}

public class PrescriptionDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string? DoctorName { get; set; }
    public string? ClinicName { get; set; }
    public string? PatientName { get; set; }
}
