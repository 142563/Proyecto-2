namespace Academic.Infrastructure.Configuration;

public sealed class AcademicOptions
{
    public int DefaultCourseCapacity { get; set; } = 40;
    public string CertificatesStoragePath { get; set; } = "storage/certificates";
}
