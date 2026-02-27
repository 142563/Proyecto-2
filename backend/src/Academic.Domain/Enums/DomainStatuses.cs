namespace Academic.Domain.Enums;

public static class DomainStatuses
{
    public static class Transfer
    {
        public const string PendingPayment = "PendingPayment";
        public const string PendingReview = "PendingReview";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Cancelled = "Cancelled";
    }

    public static class Enrollment
    {
        public const string PendingPayment = "PendingPayment";
        public const string Confirmed = "Confirmed";
        public const string Cancelled = "Cancelled";
    }

    public static class Payment
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Cancelled = "Cancelled";
    }

    public static class Certificate
    {
        public const string Requested = "Requested";
        public const string PdfGenerated = "PdfGenerated";
        public const string Sent = "Sent";
        public const string Cancelled = "Cancelled";
    }
}
