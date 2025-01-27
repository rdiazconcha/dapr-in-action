using Dapr;
using Dapr.Client;
using Hospital.Consultations.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Consultations.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultationsController(ConsultationsDbContext dbContext,
                                     DaprClient daprClient) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<Consultation>> Start(StartConsultation newConsultation)
    {
        if (await dbContext.Patients.FindAsync(newConsultation.PatientId) == null)
        {
            return BadRequest("Patient doesn't exist.");
        }

        var consultation = newConsultation.ToConsultation();
        await dbContext.Consultations.AddAsync(consultation);
        await dbContext.SaveChangesAsync();
        return Ok(consultation);
    }

    [HttpPost("end")]
    public async Task<ActionResult<Consultation>> End(EndConsultation endConsultation)
    {
        var consultation = await dbContext.Consultations.FindAsync(endConsultation.Id);
        consultation!.EndedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return Ok(consultation);
    }


    [Topic("pubsub", "patients")]
    [HttpPost]
    public async Task<IActionResult> OnPatientCreated(PatientCreated patientCreated)
    {
        var newPatient = new Infrastructure.PatientCreated(patientCreated.Id,
                                     patientCreated.CreatedAt);
        await dbContext.Patients.AddAsync(newPatient);
        await dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients()
    {
        var all = await dbContext.Patients.ToListAsync();
        return Ok(all);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConsultation(int id)
    {
        var consultation = await dbContext.Consultations.FindAsync(id);
        var patientResult = await daprClient.InvokeMethodAsync<Patient>(HttpMethod.Get, "patients", $"patients/{consultation.PatientId}");
        if (patientResult == null)
        {
            return BadRequest("Patient doesn't exist.");
        }

        return Ok(new
        {
            consultation.Id,
            consultation.StartedAt,
            consultation.EndedAt,
            consultation.DoctorId,
            @Patient = new
            {
                consultation.PatientId,
                patientResult.FirstName,
                patientResult.LastName
            }
        });
    }
}

public record StartConsultation(Guid PatientId, int DoctorId)
{
    public Consultation ToConsultation()
    {
        var consultation = new Consultation
        {
            PatientId = PatientId,
            DoctorId = DoctorId,
            StartedAt = DateTime.UtcNow
        };
        return consultation;
    }
}

public record EndConsultation(int Id);