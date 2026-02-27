CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Catalog tables
CREATE TABLE IF NOT EXISTS roles (
    id          SMALLSERIAL PRIMARY KEY,
    name        VARCHAR(32) NOT NULL UNIQUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_roles_name CHECK (name IN ('Admin', 'Student'))
);

CREATE TABLE IF NOT EXISTS programs (
    id          SERIAL PRIMARY KEY,
    code        VARCHAR(20) NOT NULL UNIQUE,
    name        VARCHAR(180) NOT NULL,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS campuses (
    id          SERIAL PRIMARY KEY,
    code        VARCHAR(20) NOT NULL UNIQUE,
    name        VARCHAR(180) NOT NULL UNIQUE,
    address     VARCHAR(280) NOT NULL,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS shifts (
    id          SMALLSERIAL PRIMARY KEY,
    name        VARCHAR(20) NOT NULL UNIQUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_shifts_name CHECK (name IN ('Saturday', 'Sunday'))
);

CREATE TABLE IF NOT EXISTS campus_shift_capacity (
    id                 BIGSERIAL PRIMARY KEY,
    campus_id          INT NOT NULL REFERENCES campuses(id) ON DELETE RESTRICT,
    shift_id           SMALLINT NOT NULL REFERENCES shifts(id) ON DELETE RESTRICT,
    total_capacity     INT NOT NULL,
    occupied_capacity  INT NOT NULL DEFAULT 0,
    updated_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_capacity_non_negative CHECK (total_capacity >= 0 AND occupied_capacity >= 0),
    CONSTRAINT chk_capacity_not_exceeded CHECK (occupied_capacity <= total_capacity),
    CONSTRAINT uq_campus_shift UNIQUE (campus_id, shift_id)
);

CREATE TABLE IF NOT EXISTS users (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email          VARCHAR(255) NOT NULL UNIQUE,
    password_hash  VARCHAR(255) NOT NULL,
    is_active      BOOLEAN NOT NULL DEFAULT TRUE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_users_email_format CHECK (POSITION('@' IN email) > 1)
);

CREATE TABLE IF NOT EXISTS user_roles (
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id     SMALLINT NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE IF NOT EXISTS students (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id             UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    student_code        VARCHAR(30) NOT NULL UNIQUE,
    institutional_email VARCHAR(255) NOT NULL UNIQUE,
    first_name          VARCHAR(120) NOT NULL,
    last_name           VARCHAR(120) NOT NULL,
    program_id          INT NOT NULL REFERENCES programs(id) ON DELETE RESTRICT,
    current_campus_id   INT NULL REFERENCES campuses(id) ON DELETE SET NULL,
    current_shift_id    SMALLINT NULL REFERENCES shifts(id) ON DELETE SET NULL,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS courses (
    id           SERIAL PRIMARY KEY,
    program_id   INT NOT NULL REFERENCES programs(id) ON DELETE RESTRICT,
    code         VARCHAR(20) NOT NULL UNIQUE,
    name         VARCHAR(180) NOT NULL,
    credits      SMALLINT NOT NULL,
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_courses_credits CHECK (credits > 0)
);

CREATE TABLE IF NOT EXISTS course_prerequisites (
    course_id               INT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    prerequisite_course_id  INT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    PRIMARY KEY (course_id, prerequisite_course_id),
    CONSTRAINT chk_course_prereq_distinct CHECK (course_id <> prerequisite_course_id)
);

CREATE TABLE IF NOT EXISTS student_course_history (
    id              BIGSERIAL PRIMARY KEY,
    student_id      UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    course_id       INT NOT NULL REFERENCES courses(id) ON DELETE RESTRICT,
    year            SMALLINT NOT NULL,
    term            VARCHAR(20) NOT NULL,
    grade           NUMERIC(5,2) NULL,
    status          VARCHAR(20) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_course_history_status CHECK (status IN ('Passed', 'Failed', 'Withdrawn')),
    CONSTRAINT chk_course_history_year CHECK (year BETWEEN 2000 AND 2100)
);

CREATE TABLE IF NOT EXISTS transfer_requests (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id       UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    from_campus_id   INT NULL REFERENCES campuses(id) ON DELETE SET NULL,
    to_campus_id     INT NOT NULL REFERENCES campuses(id) ON DELETE RESTRICT,
    to_shift_id      SMALLINT NOT NULL REFERENCES shifts(id) ON DELETE RESTRICT,
    reason           VARCHAR(500) NULL,
    status           VARCHAR(30) NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_transfer_status CHECK (status IN ('PendingPayment', 'PendingReview', 'Approved', 'Rejected', 'Cancelled')),
    CONSTRAINT chk_transfer_campus_change CHECK (from_campus_id IS NULL OR from_campus_id <> to_campus_id)
);

CREATE TABLE IF NOT EXISTS enrollments (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id       UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    enrollment_type  VARCHAR(20) NOT NULL,
    status           VARCHAR(30) NOT NULL,
    total_amount     NUMERIC(12,2) NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_enrollment_type CHECK (enrollment_type IN ('Extra', 'Overdue', 'Mixed')),
    CONSTRAINT chk_enrollment_status CHECK (status IN ('PendingPayment', 'Confirmed', 'Cancelled')),
    CONSTRAINT chk_enrollment_amount CHECK (total_amount >= 0)
);

CREATE TABLE IF NOT EXISTS enrollment_courses (
    enrollment_id  UUID NOT NULL REFERENCES enrollments(id) ON DELETE CASCADE,
    course_id      INT NOT NULL REFERENCES courses(id) ON DELETE RESTRICT,
    is_overdue     BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY (enrollment_id, course_id)
);

CREATE TABLE IF NOT EXISTS payment_orders (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id     UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    order_type     VARCHAR(20) NOT NULL,
    reference_id   UUID NOT NULL,
    amount         NUMERIC(12,2) NOT NULL,
    status         VARCHAR(20) NOT NULL,
    description    VARCHAR(255) NOT NULL,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    paid_at        TIMESTAMPTZ NULL,
    cancelled_at   TIMESTAMPTZ NULL,
    CONSTRAINT chk_payment_order_type CHECK (order_type IN ('Transfer', 'Enrollment', 'Certificate')),
    CONSTRAINT chk_payment_order_status CHECK (status IN ('Pending', 'Paid', 'Cancelled')),
    CONSTRAINT chk_payment_amount_non_negative CHECK (amount >= 0),
    CONSTRAINT uq_payment_reference UNIQUE (order_type, reference_id)
);

CREATE TABLE IF NOT EXISTS certificates (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id        UUID NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    payment_order_id  UUID NOT NULL UNIQUE REFERENCES payment_orders(id) ON DELETE RESTRICT,
    purpose           VARCHAR(255) NOT NULL,
    status            VARCHAR(30) NOT NULL,
    verification_code VARCHAR(60) NOT NULL UNIQUE,
    pdf_path          VARCHAR(500) NULL,
    metadata          JSONB NULL,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    generated_at      TIMESTAMPTZ NULL,
    sent_at           TIMESTAMPTZ NULL,
    CONSTRAINT chk_certificate_status CHECK (status IN ('Requested', 'PdfGenerated', 'Sent', 'Cancelled'))
);

CREATE TABLE IF NOT EXISTS pricing_catalog (
    id            SERIAL PRIMARY KEY,
    service_type  VARCHAR(30) NOT NULL,
    program_id    INT NULL REFERENCES programs(id) ON DELETE SET NULL,
    amount        NUMERIC(12,2) NOT NULL,
    currency      VARCHAR(3) NOT NULL DEFAULT 'USD',
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_pricing_service_type CHECK (service_type IN ('Transfer', 'Enrollment', 'CourseExtra', 'CourseOverdue', 'Certificate')),
    CONSTRAINT chk_pricing_amount CHECK (amount >= 0),
    CONSTRAINT chk_pricing_currency CHECK (currency ~ '^[A-Z]{3}$')
);

CREATE TABLE IF NOT EXISTS audit_logs (
    id           BIGSERIAL PRIMARY KEY,
    user_id      UUID NULL REFERENCES users(id) ON DELETE SET NULL,
    action       VARCHAR(100) NOT NULL,
    entity_name  VARCHAR(100) NOT NULL,
    entity_id    VARCHAR(100) NOT NULL,
    details      JSONB NULL,
    ip_address   VARCHAR(64) NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_students_user_id ON students(user_id);
CREATE INDEX IF NOT EXISTS idx_students_program_id ON students(program_id);
CREATE INDEX IF NOT EXISTS idx_courses_program_id ON courses(program_id);
CREATE INDEX IF NOT EXISTS idx_history_student_status ON student_course_history(student_id, status);
CREATE INDEX IF NOT EXISTS idx_history_course_student ON student_course_history(course_id, student_id);
CREATE INDEX IF NOT EXISTS idx_transfers_student_status ON transfer_requests(student_id, status);
CREATE INDEX IF NOT EXISTS idx_transfers_destination ON transfer_requests(to_campus_id, to_shift_id, status);
CREATE INDEX IF NOT EXISTS idx_enrollments_student_status ON enrollments(student_id, status);
CREATE INDEX IF NOT EXISTS idx_payment_orders_student_status ON payment_orders(student_id, status);
CREATE INDEX IF NOT EXISTS idx_payment_orders_reference ON payment_orders(reference_id, order_type);
CREATE INDEX IF NOT EXISTS idx_certificates_code ON certificates(verification_code);
CREATE INDEX IF NOT EXISTS idx_certificates_student_status ON certificates(student_id, status);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_created ON audit_logs(user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity_created ON audit_logs(entity_name, entity_id, created_at DESC);

-- Partial unique constraints for active requests
CREATE UNIQUE INDEX IF NOT EXISTS uq_transfer_active_per_student
    ON transfer_requests(student_id)
    WHERE status IN ('PendingPayment', 'PendingReview');

CREATE UNIQUE INDEX IF NOT EXISTS uq_enrollment_active_per_student
    ON enrollments(student_id)
    WHERE status = 'PendingPayment';

CREATE UNIQUE INDEX IF NOT EXISTS uq_certificate_active_per_student
    ON certificates(student_id)
    WHERE status IN ('Requested', 'PdfGenerated');

CREATE UNIQUE INDEX IF NOT EXISTS uq_active_pricing_service_program
    ON pricing_catalog(service_type, COALESCE(program_id, -1))
    WHERE is_active = TRUE;