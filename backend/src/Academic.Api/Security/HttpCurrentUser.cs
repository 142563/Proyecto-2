using System.Security.Claims;
using Academic.Application.Abstractions;

namespace Academic.Api.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId => ParseGuid("uid") ?? ParseGuid(ClaimTypes.NameIdentifier) ?? ParseGuid(ClaimTypes.Name);

    public Guid? StudentId => ParseGuid("studentId");

    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email");

    private Guid? ParseGuid(string claimType)
    {
        var value = Principal?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
