namespace Academic.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public string Key { get; set; } = "ChangeThisSuperSecretKey_AtLeast32Chars";
    public string Issuer { get; set; } = "Academic.Api";
    public string Audience { get; set; } = "Academic.Client";
    public int ExpirationMinutes { get; set; } = 120;
}
