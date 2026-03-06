namespace Academic.Infrastructure.Configuration;

public sealed class AcademicOptions
{
    public int DefaultCourseCapacity { get; set; } = 40;
    public string CertificatesStoragePath { get; set; } = "storage/certificates";
    public int PendingPaymentExpirationHours { get; set; } = 72;
    public string DefaultCurrency { get; set; } = "GTQ";
}
