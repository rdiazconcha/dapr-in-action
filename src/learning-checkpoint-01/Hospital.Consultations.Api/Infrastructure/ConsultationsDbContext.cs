using Microsoft.EntityFrameworkCore;

namespace Hospital.Consultations.Api.Infrastructure;

public class ConsultationsDbContext(DbContextOptions<ConsultationsDbContext> options) : DbContext(options)
{
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Consultation> Consultations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Specialty>().HasData
            (
                new Specialty(1, "Dermatology"),
                new Specialty(2, "Ophthalmology ")
            );
        modelBuilder.Entity<Doctor>().HasData
            (
                new Doctor() { Id = 1, Name = "House", SpecialtyId = 1 },
                new Doctor() { Id = 2, Name = "Strange", SpecialtyId = 2 }
            );
    }
}

public class Consultation
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
}

public record Specialty(int Id, string Name);

public class Doctor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; }
}

public static class ConsultationsDbContextExtensions
{
    public static void EnsureConsultationsDbIsCreated(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetService<ConsultationsDbContext>();
        context!.Database.EnsureCreated();
    }
}