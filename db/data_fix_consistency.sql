-- Cancels stale pending records and repairs orphan states.
-- Safe to execute multiple times.
DO $$
DECLARE
    expired_payments_count INTEGER := 0;
    transfer_cancel_count INTEGER := 0;
    transfer_orphan_count INTEGER := 0;
    enrollment_cancel_count INTEGER := 0;
    enrollment_orphan_count INTEGER := 0;
    certificate_cancel_count INTEGER := 0;
    certificate_orphan_count INTEGER := 0;
BEGIN
    UPDATE payment_orders
    SET expires_at = created_at + INTERVAL '72 hours'
    WHERE status = 'Pending'
      AND (expires_at IS NULL OR expires_at > created_at + INTERVAL '72 hours');

    UPDATE payment_orders
    SET status = 'Cancelled',
        cancelled_at = NOW()
    WHERE status = 'Pending'
      AND COALESCE(expires_at, created_at + INTERVAL '72 hours') < NOW();
    GET DIAGNOSTICS expired_payments_count = ROW_COUNT;

    UPDATE transfer_requests tr
    SET status = 'Cancelled',
        updated_at = NOW(),
        review_notes = CONCAT(COALESCE(tr.review_notes, ''),
            CASE WHEN COALESCE(tr.review_notes, '') = '' THEN '' ELSE ' | ' END,
            'Auto-cancelled due to expired payment')
    WHERE tr.status IN ('PendingPayment', 'PendingReview')
      AND EXISTS (
          SELECT 1
          FROM payment_orders po
          WHERE po.order_type = 'Transfer'
            AND po.reference_id = tr.id
            AND po.status = 'Cancelled'
      );
    GET DIAGNOSTICS transfer_cancel_count = ROW_COUNT;

    UPDATE transfer_requests tr
    SET status = 'Cancelled',
        updated_at = NOW(),
        review_notes = CONCAT(COALESCE(tr.review_notes, ''),
            CASE WHEN COALESCE(tr.review_notes, '') = '' THEN '' ELSE ' | ' END,
            'Auto-cancelled due to orphan state')
    WHERE (
        tr.status = 'PendingPayment'
        AND NOT EXISTS (
            SELECT 1 FROM payment_orders po
            WHERE po.order_type = 'Transfer'
              AND po.reference_id = tr.id
              AND po.status = 'Pending'
        )
    )
    OR (
        tr.status = 'PendingReview'
        AND NOT EXISTS (
            SELECT 1 FROM payment_orders po
            WHERE po.order_type = 'Transfer'
              AND po.reference_id = tr.id
              AND po.status = 'Paid'
        )
    );
    GET DIAGNOSTICS transfer_orphan_count = ROW_COUNT;

    UPDATE enrollments e
    SET status = 'Cancelled',
        updated_at = NOW()
    WHERE e.status = 'PendingPayment'
      AND EXISTS (
          SELECT 1
          FROM payment_orders po
          WHERE po.order_type = 'Enrollment'
            AND po.reference_id = e.id
            AND po.status = 'Cancelled'
      );
    GET DIAGNOSTICS enrollment_cancel_count = ROW_COUNT;

    UPDATE enrollments e
    SET status = 'Cancelled',
        updated_at = NOW()
    WHERE e.status = 'PendingPayment'
      AND NOT EXISTS (
          SELECT 1
          FROM payment_orders po
          WHERE po.order_type = 'Enrollment'
            AND po.reference_id = e.id
            AND po.status = 'Pending'
      );
    GET DIAGNOSTICS enrollment_orphan_count = ROW_COUNT;

    UPDATE certificates c
    SET status = 'Cancelled'
    WHERE c.status IN ('Requested', 'PdfGenerated')
      AND EXISTS (
          SELECT 1
          FROM payment_orders po
          WHERE po.order_type = 'Certificate'
            AND po.reference_id = c.id
            AND po.status = 'Cancelled'
      );
    GET DIAGNOSTICS certificate_cancel_count = ROW_COUNT;

    UPDATE certificates c
    SET status = 'Cancelled'
    WHERE (
        c.status = 'Requested'
        AND NOT EXISTS (
            SELECT 1
            FROM payment_orders po
            WHERE po.order_type = 'Certificate'
              AND po.reference_id = c.id
              AND po.status IN ('Pending', 'Paid')
        )
    )
    OR (
        c.status = 'PdfGenerated'
        AND NOT EXISTS (
            SELECT 1
            FROM payment_orders po
            WHERE po.order_type = 'Certificate'
              AND po.reference_id = c.id
              AND po.status = 'Paid'
        )
    );
    GET DIAGNOSTICS certificate_orphan_count = ROW_COUNT;

    INSERT INTO audit_logs (user_id, action, entity_name, entity_id, details, ip_address)
    VALUES (
        NULL,
        'ConsistencyFixExecuted',
        'Database',
        'data_fix_consistency',
        jsonb_build_object(
            'expiredPayments', expired_payments_count,
            'transfersCancelledByPayment', transfer_cancel_count,
            'transfersCancelledOrphan', transfer_orphan_count,
            'enrollmentsCancelledByPayment', enrollment_cancel_count,
            'enrollmentsCancelledOrphan', enrollment_orphan_count,
            'certificatesCancelledByPayment', certificate_cancel_count,
            'certificatesCancelledOrphan', certificate_orphan_count,
            'executedAtUtc', NOW()
        ),
        '127.0.0.1'
    );
END $$;
