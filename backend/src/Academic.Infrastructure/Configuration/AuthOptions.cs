namespace Academic.Infrastructure.Configuration;

public sealed class AuthOptions
{
    public string[] AllowedEmailDomains { get; set; } =
    [
        "umg.edu.gt",
        "alumnos.umg.edu.gt",
        "universidad.edu",
        "alumnos.universidad.edu"
    ];
}
