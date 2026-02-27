using Academic.Domain.Entities;
using Academic.Infrastructure.Persistence.Scaffold;

namespace Academic.Infrastructure.Adapters;

public static class EntityAdapters
{
    public static AcademicUser ToDomain(this User user, string role)
        => new(user.Id, user.Email, user.IsActive, role, user.Student?.Id);
}
