using Hospital.Consultations.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

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