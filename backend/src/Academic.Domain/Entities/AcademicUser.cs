namespace Academic.Domain.Entities;

public sealed record AcademicUser(Guid Id, string Email, bool IsActive, string Role, Guid? StudentId);
