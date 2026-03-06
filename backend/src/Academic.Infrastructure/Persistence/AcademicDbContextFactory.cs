using Academic.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Academic.Infrastructure.Persistence;

public sealed class AcademicDbContextFactory : IDesignTimeDbContextFactory<AcademicDbContext>
{
    public AcademicDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings:Default")
            ?? "Host=localhost;Port=5432;Database=academic_portal;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<AcademicDbContext>();
        optionsBuilder.UseNpgsql(ConnectionStringParser.Normalize(connectionString));
        return new AcademicDbContext(optionsBuilder.Options);
    }
}
