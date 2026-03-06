using Academic.Domain.ValueObjects;

namespace Academic.Domain.Tests;

public class InstitutionalEmailTests
{
    [Fact]
    public void TryCreate_ShouldReturnTrue_ForAllowedUmgDomain()
    {
        var ok = InstitutionalEmail.TryCreate(
            "ana.gomez@alumnos.umg.edu.gt",
            ["umg.edu.gt", "alumnos.umg.edu.gt", "universidad.edu", "alumnos.universidad.edu"],
            out var email);

        Assert.True(ok);
        Assert.NotNull(email);
        Assert.Equal("ana.gomez@alumnos.umg.edu.gt", email!.Value);
    }

    [Fact]
    public void TryCreate_ShouldReturnTrue_ForCompatibilityDomain()
    {
        var ok = InstitutionalEmail.TryCreate(
            "ana.gomez@alumnos.universidad.edu",
            ["umg.edu.gt", "alumnos.umg.edu.gt", "universidad.edu", "alumnos.universidad.edu"],
            out var email);

        Assert.True(ok);
        Assert.NotNull(email);
    }

    [Fact]
    public void TryCreate_ShouldReturnFalse_ForDomainOutsideWhitelist()
    {
        var ok = InstitutionalEmail.TryCreate(
            "intruso@gmail.com",
            ["umg.edu.gt", "alumnos.umg.edu.gt", "universidad.edu", "alumnos.universidad.edu"],
            out var email);

        Assert.False(ok);
        Assert.Null(email);
    }
}
