using Dapr.Client;
using Hospital.Patients.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Patients.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PatientsController(PatientsDbContext dbContext,
                                DaprClient daprClient) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(NewPatient newPatient)
    {
        var patient = newPatient.ToPatient();
        await dbContext.Patients.AddAsync(patient);
        await dbContext.SaveChangesAsync();
        await daprClient.PublishEventAsync("pubsub", "patients",
            new PatientCreated(patient.Id,
                               DateTime.UtcNow));
        return Ok(patient.Id);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await dbContext.Patients.ToListAsync();
        return Ok(all);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patient = await dbContext.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound();
        }
        return Ok(patient);
    }
}

public record NewPatient(string FirstName, string LastName)
{
    public Patient ToPatient()
    {
        return new Patient()
        {
            FirstName = FirstName,
            LastName = LastName
        };
    }
}

public record PatientCreated(Guid Id, DateTime CreatedAt);