-- Roles
INSERT INTO roles (id, name) VALUES
(1, 'Admin'),
(2, 'Student')
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name;

-- Shifts
INSERT INTO shifts (id, name) VALUES
(1, 'Saturday'),
(2, 'Sunday')
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name;

-- Programs
INSERT INTO programs (id, code, name, is_active) VALUES
(1, 'SWE', 'Ingenieria de Software', TRUE)
ON CONFLICT (id) DO UPDATE SET
    code = EXCLUDED.code,
    name = EXCLUDED.name,
    is_active = EXCLUDED.is_active;

-- UMG campuses and centers
INSERT INTO campuses (id, code, name, address, campus_type, region, is_active) VALUES
(1, 'UMG-CENTRAL', 'Campus Central', '7a Avenida 13-27, Zona 9, Ciudad de Guatemala', 'Campus', 'Guatemala', TRUE),
(2, 'UMG-HUEHUE', 'Campus Huehuetenango', '2a Calle 8-40, Zona 1, Huehuetenango', 'Campus', 'Huehuetenango', TRUE),
(3, 'UMG-JUTIAPA', 'Campus Jutiapa', '2a Avenida 5-45, Zona 1, Jutiapa', 'Campus', 'Jutiapa', TRUE),
(4, 'UMG-QUETZAL', 'Campus Quetzaltenango', '14 Avenida 1-35, Zona 3, Quetzaltenango', 'Campus', 'Quetzaltenango', TRUE),
(5, 'UMG-VNUEVA', 'Campus Villa Nueva', '4a Calle 7-22, Zona 1, Villa Nueva', 'Campus', 'Guatemala', TRUE),
(6, 'UMG-ANTIGUA', 'Campus Antigua Guatemala', '5a Calle Poniente 18, Antigua Guatemala', 'Campus', 'Sacatepequez', TRUE),
(7, 'UMG-AMATI', 'Centro Universitario Amatitlan', '2a Avenida 1-60, Amatitlan', 'Centro', 'Guatemala', TRUE),
(8, 'UMG-BOCAM', 'Centro Universitario Boca del Monte', '5a Avenida 0-70, Boca del Monte, Villa Canales', 'Centro', 'Guatemala', TRUE),
(9, 'UMG-CHINA', 'Centro Universitario Chinautla', 'Km 12.5, Ruta a Chinautla', 'Centro', 'Guatemala', TRUE),
(10, 'UMG-FLOR19', 'Centro Universitario La Florida Zona 19', 'Boulevard La Florida 18-40, Zona 19', 'Centro', 'Guatemala', TRUE),
(11, 'UMG-NARAN', 'Centro Universitario El Naranjo', 'Calzada El Naranjo 22-18, Mixco', 'Centro', 'Guatemala', TRUE),
(12, 'UMG-PORTA', 'Centro Universitario Portales', '14 Avenida 13-80, Zona 17', 'Centro', 'Guatemala', TRUE),
(13, 'UMG-PINULA', 'Centro Universitario San Jose Pinula', '3a Calle 4-25, San Jose Pinula', 'Centro', 'Guatemala', TRUE),
(14, 'UMG-Z16', 'Centro Universitario Zona 16 Acatan', 'Boulevard Acatan 17-45, Zona 16', 'Centro', 'Guatemala', TRUE),
(15, 'UMG-SJSAC', 'Centro Universitario San Juan Sacatepequez', '1a Calle 3-15, Zona 1, San Juan Sacatepequez', 'Centro', 'Guatemala', TRUE),
(16, 'UMG-CHIMAL', 'Centro Universitario Chimaltenango', '1a Avenida 4-20, Zona 2, Chimaltenango', 'Centro', 'Chimaltenango', TRUE),
(17, 'UMG-ESCUINT', 'Centro Universitario Escuintla', '5a Avenida 12-14, Zona 1, Escuintla', 'Centro', 'Escuintla', TRUE),
(18, 'UMG-SLUCIA', 'Centro Universitario Santa Lucia Cotzumalguapa', '6a Calle 2-30, Zona 1, Santa Lucia Cotzumalguapa', 'Centro', 'Escuintla', TRUE)
ON CONFLICT (id) DO UPDATE SET
    code = EXCLUDED.code,
    name = EXCLUDED.name,
    address = EXCLUDED.address,
    campus_type = EXCLUDED.campus_type,
    region = EXCLUDED.region,
    is_active = EXCLUDED.is_active;

-- Capacity by campus/shift
INSERT INTO campus_shift_capacity (campus_id, shift_id, total_capacity, occupied_capacity) VALUES
(1, 1, 250, 150), (1, 2, 250, 140),
(2, 1, 180, 90), (2, 2, 180, 88),
(3, 1, 160, 70), (3, 2, 160, 66),
(4, 1, 190, 95), (4, 2, 190, 94),
(5, 1, 200, 100), (5, 2, 200, 96),
(6, 1, 140, 72), (6, 2, 140, 60),
(7, 1, 120, 45), (7, 2, 120, 44),
(8, 1, 110, 42), (8, 2, 110, 39),
(9, 1, 100, 40), (9, 2, 100, 38),
(10, 1, 95, 35), (10, 2, 95, 33),
(11, 1, 115, 46), (11, 2, 115, 41),
(12, 1, 105, 40), (12, 2, 105, 36),
(13, 1, 95, 33), (13, 2, 95, 31),
(14, 1, 100, 38), (14, 2, 100, 34),
(15, 1, 90, 29), (15, 2, 90, 28),
(16, 1, 120, 48), (16, 2, 120, 45),
(17, 1, 130, 52), (17, 2, 130, 50),
(18, 1, 110, 42), (18, 2, 110, 39)
ON CONFLICT (campus_id, shift_id) DO UPDATE SET
    total_capacity = EXCLUDED.total_capacity,
    occupied_capacity = EXCLUDED.occupied_capacity,
    updated_at = NOW();

-- Users (passwords: Admin123! / Student123!)
INSERT INTO users (id, email, password_hash, is_active) VALUES
('11111111-1111-1111-1111-111111111111', 'admin@umg.edu.gt', crypt('Admin123!', gen_salt('bf')), TRUE),
('22222222-2222-2222-2222-222222222222', 'ana.gomez@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE),
('33333333-3333-3333-3333-333333333333', 'juan.perez@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE)
ON CONFLICT (id) DO UPDATE SET
    email = EXCLUDED.email,
    password_hash = EXCLUDED.password_hash,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Roles for users
INSERT INTO user_roles (user_id, role_id) VALUES
('11111111-1111-1111-1111-111111111111', 1),
('22222222-2222-2222-2222-222222222222', 2),
('33333333-3333-3333-3333-333333333333', 2)
ON CONFLICT (user_id, role_id) DO NOTHING;

-- Students
INSERT INTO students (id, user_id, student_code, institutional_email, first_name, last_name, program_id, current_campus_id, current_shift_id, is_active) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-2222-2222-2222-222222222222', '20261001', 'ana.gomez@alumnos.umg.edu.gt', 'Ana', 'Gomez', 1, 1, 1, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '33333333-3333-3333-3333-333333333333', '20261002', 'juan.perez@alumnos.umg.edu.gt', 'Juan', 'Perez', 1, 5, 2, TRUE)
ON CONFLICT (id) DO UPDATE SET
    user_id = EXCLUDED.user_id,
    student_code = EXCLUDED.student_code,
    institutional_email = EXCLUDED.institutional_email,
    first_name = EXCLUDED.first_name,
    last_name = EXCLUDED.last_name,
    program_id = EXCLUDED.program_id,
    current_campus_id = EXCLUDED.current_campus_id,
    current_shift_id = EXCLUDED.current_shift_id,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Courses
INSERT INTO courses (id, program_id, code, name, credits, is_active) VALUES
(1, 1, 'MAT101', 'Matematica I', 3, TRUE),
(2, 1, 'PRO101', 'Programacion I', 4, TRUE),
(3, 1, 'PRO201', 'Estructuras de Datos', 4, TRUE),
(4, 1, 'DB101', 'Bases de Datos', 3, TRUE),
(5, 1, 'WEB201', 'Desarrollo Web', 3, TRUE),
(6, 1, 'ARQ301', 'Arquitectura de Software', 4, TRUE)
ON CONFLICT (id) DO UPDATE SET
    program_id = EXCLUDED.program_id,
    code = EXCLUDED.code,
    name = EXCLUDED.name,
    credits = EXCLUDED.credits,
    is_active = EXCLUDED.is_active;

-- Prerequisites
INSERT INTO course_prerequisites (course_id, prerequisite_course_id) VALUES
(3, 2),
(5, 2),
(6, 3),
(6, 4)
ON CONFLICT (course_id, prerequisite_course_id) DO NOTHING;

-- Course history
INSERT INTO student_course_history (student_id, course_id, year, term, grade, status) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 1, 2025, '2025-1', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 2, 2025, '2025-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 3, 2025, '2025-2', 52.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 1, 2025, '2025-1', 78.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 2, 2025, '2025-2', 56.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 4, 2025, '2025-2', 82.00, 'Passed')
ON CONFLICT DO NOTHING;

-- Pricing catalog in GTQ
UPDATE pricing_catalog
SET is_active = FALSE
WHERE service_type IN ('Transfer', 'Enrollment', 'CourseExtra', 'CourseOverdue', 'Certificate')
  AND is_active = TRUE;

INSERT INTO pricing_catalog (service_type, program_id, amount, currency, is_active) VALUES
('Transfer', NULL, 150.00, 'GTQ', TRUE),
('Enrollment', 1, 60.00, 'GTQ', TRUE),
('CourseExtra', 1, 175.00, 'GTQ', TRUE),
('CourseOverdue', 1, 130.00, 'GTQ', TRUE),
('Certificate', NULL, 70.00, 'GTQ', TRUE)
ON CONFLICT DO NOTHING;

-- Audit seed execution
INSERT INTO audit_logs (user_id, action, entity_name, entity_id, details, ip_address)
VALUES
('11111111-1111-1111-1111-111111111111', 'SeedExecuted', 'Database', 'initial_seed', '{"source":"seed.sql","institution":"UMG"}'::jsonb, '127.0.0.1');
