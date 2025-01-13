using Hospital.Consultations.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
                .AddDapr();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ConsultationsDbContext>(options =>
{
    options.UseInMemoryDatabase("consultations");
});
var app = builder.Build();

app.EnsureConsultationsDbIsCreated();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapSubscribeHandler();
app.UseCloudEvents();
app.MapControllers();

app.Run();