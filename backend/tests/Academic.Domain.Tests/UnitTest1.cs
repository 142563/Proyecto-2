using Academic.Domain.ValueObjects;

namespace Academic.Domain.Tests;

public class InstitutionalEmailTests
{
    [Fact]
    public void TryCreate_ShouldReturnTrue_ForAllowedDomain()
    {
        var ok = InstitutionalEmail.TryCreate(
            "ana.gomez@alumnos.universidad.edu",
            ["universidad.edu", "alumnos.universidad.edu"],
            out var email);

        Assert.True(ok);
        Assert.NotNull(email);
        Assert.Equal("ana.gomez@alumnos.universidad.edu", email!.Value);
    }

    [Fact]
    public void TryCreate_ShouldReturnFalse_ForDomainOutsideWhitelist()
    {
        var ok = InstitutionalEmail.TryCreate(
            "intruso@gmail.com",
            ["universidad.edu", "alumnos.universidad.edu"],
            out var email);

        Assert.False(ok);
        Assert.Null(email);
    }
}
