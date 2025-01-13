using Dapr;
using Hospital.Consultations.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Consultations.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultationsController(ConsultationsDbContext dbContext) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<Consultation>> Start(StartConsultation newConsultation)
    {
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
        var newPatient = new Patient(patientCreated.Id,
                                     patientCreated.FirstName,
                                     patientCreated.LastName);
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
}

public record StartConsultation(int PatientId, int DoctorId)
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

public record PatientCreated(int Id, string FirstName, string LastName);