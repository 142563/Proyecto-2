-- Roles
INSERT INTO roles (id, name) VALUES
(1, 'Admin'),
(2, 'Student')
ON CONFLICT (id) DO NOTHING;

-- Shifts
INSERT INTO shifts (id, name) VALUES
(1, 'Saturday'),
(2, 'Sunday')
ON CONFLICT (id) DO NOTHING;

-- Programs
INSERT INTO programs (id, code, name) VALUES
(1, 'SWE', 'Ingenieria de Software')
ON CONFLICT (id) DO NOTHING;

-- Campuses
INSERT INTO campuses (id, code, name, address) VALUES
(1, 'CAMPUS-NORTE', 'Sede Norte', 'Av. Principal 100'),
(2, 'CAMPUS-SUR', 'Sede Sur', 'Calle 45 #12-00'),
(3, 'CAMPUS-CENTRO', 'Sede Centro', 'Cra. 10 #20-30')
ON CONFLICT (id) DO NOTHING;

-- Capacity per campus/shift
INSERT INTO campus_shift_capacity (campus_id, shift_id, total_capacity, occupied_capacity) VALUES
(1, 1, 120, 80),
(1, 2, 120, 90),
(2, 1, 100, 60),
(2, 2, 100, 70),
(3, 1, 90, 40),
(3, 2, 90, 50)
ON CONFLICT (campus_id, shift_id) DO NOTHING;

-- Users (password = Admin123! / Student123! generated with pgcrypto crypt+bf)
INSERT INTO users (id, email, password_hash, is_active) VALUES
('11111111-1111-1111-1111-111111111111', 'admin@universidad.edu', crypt('Admin123!', gen_salt('bf')), TRUE),
('22222222-2222-2222-2222-222222222222', 'ana.gomez@alumnos.universidad.edu', crypt('Student123!', gen_salt('bf')), TRUE),
('33333333-3333-3333-3333-333333333333', 'juan.perez@alumnos.universidad.edu', crypt('Student123!', gen_salt('bf')), TRUE)
ON CONFLICT (id) DO NOTHING;

-- Roles for users
INSERT INTO user_roles (user_id, role_id)
VALUES
('11111111-1111-1111-1111-111111111111', 1),
('22222222-2222-2222-2222-222222222222', 2),
('33333333-3333-3333-3333-333333333333', 2)
ON CONFLICT (user_id, role_id) DO NOTHING;

-- Students
INSERT INTO students (id, user_id, student_code, institutional_email, first_name, last_name, program_id, current_campus_id, current_shift_id, is_active) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-2222-2222-2222-222222222222', '20261001', 'ana.gomez@alumnos.universidad.edu', 'Ana', 'Gomez', 1, 1, 1, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '33333333-3333-3333-3333-333333333333', '20261002', 'juan.perez@alumnos.universidad.edu', 'Juan', 'Perez', 1, 2, 2, TRUE)
ON CONFLICT (id) DO NOTHING;

-- Courses
INSERT INTO courses (id, program_id, code, name, credits, is_active) VALUES
(1, 1, 'MAT101', 'Matematicas I', 3, TRUE),
(2, 1, 'PRO101', 'Programacion I', 4, TRUE),
(3, 1, 'PRO201', 'Estructuras de Datos', 4, TRUE),
(4, 1, 'DB101', 'Bases de Datos', 3, TRUE),
(5, 1, 'WEB201', 'Desarrollo Web', 3, TRUE),
(6, 1, 'ARQ301', 'Arquitectura de Software', 4, TRUE)
ON CONFLICT (id) DO NOTHING;

-- Prerequisites
INSERT INTO course_prerequisites (course_id, prerequisite_course_id) VALUES
(3, 2),
(5, 2),
(6, 3),
(6, 4)
ON CONFLICT (course_id, prerequisite_course_id) DO NOTHING;

-- Course history (includes overdue candidates)
INSERT INTO student_course_history (student_id, course_id, year, term, grade, status) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 1, 2025, '2025-1', 4.1, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 2, 2025, '2025-1', 4.3, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 3, 2025, '2025-2', 2.4, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 1, 2025, '2025-1', 3.8, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 2, 2025, '2025-2', 2.7, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 4, 2025, '2025-2', 3.9, 'Passed')
ON CONFLICT DO NOTHING;

-- Pricing catalog
INSERT INTO pricing_catalog (service_type, program_id, amount, currency, is_active) VALUES
('Transfer', NULL, 25.00, 'USD', TRUE),
('Enrollment', 1, 10.00, 'USD', TRUE),
('CourseExtra', 1, 35.00, 'USD', TRUE),
('CourseOverdue', 1, 20.00, 'USD', TRUE),
('Certificate', NULL, 15.00, 'USD', TRUE)
ON CONFLICT DO NOTHING;

-- Optional sample payment order for testing reports
INSERT INTO payment_orders (id, student_id, order_type, reference_id, amount, status, description)
VALUES
('44444444-4444-4444-4444-444444444444', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Certificate', '55555555-5555-5555-5555-555555555555', 15.00, 'Pending', 'Solicitud certificacion digital')
ON CONFLICT (id) DO NOTHING;

INSERT INTO audit_logs (user_id, action, entity_name, entity_id, details, ip_address)
VALUES
('11111111-1111-1111-1111-111111111111', 'SeedExecuted', 'Database', 'initial_seed', '{"source":"seed.sql"}'::jsonb, '127.0.0.1');