namespace Academic.Domain.ValueObjects;

public sealed class InstitutionalEmail
{
    public string Value { get; }

    private InstitutionalEmail(string value)
    {
        Value = value;
    }

    public static bool TryCreate(string input, IReadOnlyCollection<string> allowedDomains, out InstitutionalEmail? email)
    {
        email = null;

        if (string.IsNullOrWhiteSpace(input) || !input.Contains('@'))
        {
            return false;
        }

        var normalized = input.Trim().ToLowerInvariant();
        var domain = normalized[(normalized.LastIndexOf('@') + 1)..];
        if (!allowedDomains.Any(d => string.Equals(d, domain, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        email = new InstitutionalEmail(normalized);
        return true;
    }
}
