namespace Academic.Infrastructure.Configuration;

public sealed class AuthOptions
{
    public string[] AllowedEmailDomains { get; set; } = ["universidad.edu", "alumnos.universidad.edu"];
}
