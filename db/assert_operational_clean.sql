DO $$
DECLARE
    v_users INT;
    v_transfers INT;
    v_enrollments INT;
    v_enrollment_courses INT;
    v_payments INT;
    v_certificates INT;
    v_audit_logs INT;
BEGIN
    SELECT COUNT(*) INTO v_users FROM users;
    SELECT COUNT(*) INTO v_transfers FROM transfer_requests;
    SELECT COUNT(*) INTO v_enrollments FROM enrollments;
    SELECT COUNT(*) INTO v_enrollment_courses FROM enrollment_courses;
    SELECT COUNT(*) INTO v_payments FROM payment_orders;
    SELECT COUNT(*) INTO v_certificates FROM certificates;
    SELECT COUNT(*) INTO v_audit_logs FROM audit_logs;

    IF v_users = 0 THEN
        RAISE EXCEPTION 'Validation failed: users table is empty.';
    END IF;

    IF v_transfers <> 0 OR v_enrollments <> 0 OR v_enrollment_courses <> 0 OR v_payments <> 0 OR v_certificates <> 0 OR v_audit_logs <> 0 THEN
        RAISE EXCEPTION 'Validation failed: operational tables are not clean (transfers %, enrollments %, enrollment_courses %, payments %, certificates %, audit_logs %).',
            v_transfers, v_enrollments, v_enrollment_courses, v_payments, v_certificates, v_audit_logs;
    END IF;
END
$$;
