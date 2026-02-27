using Academic.Application.Abstractions;
using Academic.Infrastructure.Configuration;
using Academic.Infrastructure.Persistence.Scaffold;
using Academic.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Academic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("Default")
            ?? configuration["ConnectionStrings:Default"]
            ?? configuration["ConnectionStrings__Default"];

        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
        }

        var normalizedConnectionString = ConnectionStringParser.Normalize(rawConnectionString);

        services.AddDbContext<AcademicDbContext>(options =>
            options.UseNpgsql(normalizedConnectionString));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<AuthOptions>(configuration.GetSection("Auth"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<AcademicOptions>(configuration.GetSection("Academic"));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddScoped<AcademicOperationsService>();
        services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AcademicOperationsService>());
        services.AddScoped<ITransferService>(sp => sp.GetRequiredService<AcademicOperationsService>());
        services.AddScoped<ICourseService>(sp => sp.GetRequiredService<AcademicOperationsService>());
        services.AddScoped<IPaymentService>(sp => sp.GetRequiredService<AcademicOperationsService>());
        services.AddScoped<ICertificateService>(sp => sp.GetRequiredService<AcademicOperationsService>());
        services.AddScoped<IReportService>(sp => sp.GetRequiredService<AcademicOperationsService>());

        return services;
    }
}
