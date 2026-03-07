-- Clears only operational/transient data while preserving users, students, catalog, and academic history.
TRUNCATE TABLE
    enrollment_courses,
    enrollments,
    transfer_requests,
    certificates,
    payment_orders,
    audit_logs
RESTART IDENTITY CASCADE;
