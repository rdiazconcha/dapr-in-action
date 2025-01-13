using Hospital.Patients.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Patients.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PatientsController(PatientsDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(NewPatient newPatient)
    {
        var patient = newPatient.ToPatient();
        await dbContext.Patients.AddAsync(patient);
        await dbContext.SaveChangesAsync();

        return Ok(patient.Id);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var all = await dbContext.Patients.ToListAsync();
        return Ok(all);
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