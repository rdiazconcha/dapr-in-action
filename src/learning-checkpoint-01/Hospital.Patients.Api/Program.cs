using Hospital.Patients.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
                .AddDapr();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PatientsDbContext>(options =>
{
    options.UseInMemoryDatabase("patients");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();