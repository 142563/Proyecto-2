namespace Academic.Infrastructure.Configuration;

public sealed class EmailOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string User { get; set; } = string.Empty;
    public string Pass { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
    public bool UseMock { get; set; } = true;
    public string From { get; set; } = "noreply@universidad.edu";
}
