namespace Academic.Infrastructure.Configuration;

public sealed class AcademicOptions
{
    public int DefaultCourseCapacity { get; set; } = 40;
    public string CertificatesStoragePath { get; set; } = "storage/certificates";
    public string EnrollmentDireStoragePath { get; set; } = "storage/dire";
    public int PendingPaymentExpirationHours { get; set; } = 72;
    public string DefaultCurrency { get; set; } = "GTQ";
}
