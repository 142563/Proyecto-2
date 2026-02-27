namespace Academic.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? StudentId { get; }
    string? Role { get; }
    string? Email { get; }
}
