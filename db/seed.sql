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
(1, 'SIS-2014', 'Ingenieria en Sistemas de Informacion y Ciencias de la Computacion', TRUE)
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

-- Carnet prefix catalog (Sistemas multi-sede)
INSERT INTO carnet_prefix_catalog (prefix, program_id, campus_id, shift_id, description, is_active) VALUES
('0901', 1, 1, 1, 'Sistemas Campus Central plan sabado', TRUE),
('0902', 1, 1, 2, 'Sistemas Campus Central plan domingo', TRUE),
('0903', 1, 4, 1, 'Sistemas Campus Quetzaltenango plan sabado', TRUE),
('0904', 1, 5, 1, 'Sistemas Campus Villa Nueva plan sabado', TRUE),
('0905', 1, 2, 1, 'Sistemas Campus Huehuetenango plan sabado', TRUE),
('0906', 1, 3, 1, 'Sistemas Campus Jutiapa plan sabado', TRUE),
('0907', 1, 6, 1, 'Sistemas Campus Antigua Guatemala plan sabado', TRUE),
('0908', 1, 17, 1, 'Sistemas Centro Escuintla plan sabado', TRUE),
('0909', 1, 17, 2, 'Sistemas Centro Escuintla plan domingo', TRUE),
('0910', 1, 16, 1, 'Sistemas Centro Chimaltenango plan sabado', TRUE)
ON CONFLICT (prefix) DO UPDATE SET
    program_id = EXCLUDED.program_id,
    campus_id = EXCLUDED.campus_id,
    shift_id = EXCLUDED.shift_id,
    description = EXCLUDED.description,
    is_active = EXCLUDED.is_active;

-- Users (passwords: Admin123! / Student123!)
INSERT INTO users (id, email, password_hash, is_active) VALUES
('11111111-1111-1111-1111-111111111111', 'admin@umg.edu.gt', crypt('Admin123!', gen_salt('bf')), TRUE),
('22222222-2222-2222-2222-222222222222', 'ana.gomez@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE),
('33333333-3333-3333-3333-333333333333', 'carlos.salazar@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE)
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

-- Reset transactional demo data to keep seed idempotent and avoid stale locks.
DELETE FROM enrollment_courses
WHERE enrollment_id IN (
    SELECT id
    FROM enrollments
    WHERE student_id IN (
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
    )
);

DELETE FROM certificates
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
);

DELETE FROM transfer_requests
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
);

DELETE FROM enrollments
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
);

DELETE FROM payment_orders
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
);

DELETE FROM audit_logs
WHERE user_id IN (
    '22222222-2222-2222-2222-222222222222',
    '33333333-3333-3333-3333-333333333333'
);

-- Students
INSERT INTO students (
    id, user_id, student_code, carnet, carnet_prefix, entry_year, carnet_sequence,
    institutional_email, first_name, last_name, program_id, current_campus_id, current_shift_id, is_active
) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-2222-2222-2222-222222222222', 'SIS-22001', '0908-22-14264', '0908', 22, '14264', 'ana.gomez@alumnos.umg.edu.gt', 'Ana Lucia', 'Gomez Morales', 1, 17, 1, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '33333333-3333-3333-3333-333333333333', 'SIS-23009', '0909-23-09876', '0909', 23, '09876', 'carlos.salazar@alumnos.umg.edu.gt', 'Carlos Eduardo', 'Salazar Lopez', 1, 17, 2, TRUE)
ON CONFLICT (id) DO UPDATE SET
    user_id = EXCLUDED.user_id,
    student_code = EXCLUDED.student_code,
    carnet = EXCLUDED.carnet,
    carnet_prefix = EXCLUDED.carnet_prefix,
    entry_year = EXCLUDED.entry_year,
    carnet_sequence = EXCLUDED.carnet_sequence,
    institutional_email = EXCLUDED.institutional_email,
    first_name = EXCLUDED.first_name,
    last_name = EXCLUDED.last_name,
    program_id = EXCLUDED.program_id,
    current_campus_id = EXCLUDED.current_campus_id,
    current_shift_id = EXCLUDED.current_shift_id,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Courses (Pensum Sistemas 2014)
INSERT INTO courses (id, program_id, code, name, cycle, credits, hours_per_week, hours_total, is_lab, is_active) VALUES
(1, 1, '090001', 'Desarrollo Humano y Profesional', 1, 4, 4, 64, FALSE, TRUE),
(2, 1, '090002', 'Metodologia de la Investigacion', 1, 5, 5, 80, FALSE, TRUE),
(3, 1, '090003', 'Contabilidad I', 1, 5, 5, 80, FALSE, TRUE),
(4, 1, '090004', 'Introduccion a los Sistemas de Computo', 1, 5, 5, 80, TRUE, TRUE),
(5, 1, '090005', 'Logica de Sistemas', 1, 5, 5, 80, FALSE, TRUE),
(6, 1, '090006', 'Precalculo', 2, 5, 5, 80, FALSE, TRUE),
(7, 1, '090007', 'Algebra Lineal', 2, 5, 5, 80, FALSE, TRUE),
(8, 1, '090008', 'Algoritmos', 2, 5, 5, 80, TRUE, TRUE),
(9, 1, '090009', 'Contabilidad II', 2, 5, 5, 80, FALSE, TRUE),
(10, 1, '090010', 'Matematica Discreta', 2, 5, 5, 80, FALSE, TRUE),
(11, 1, '090011', 'Fisica I', 3, 5, 5, 80, FALSE, TRUE),
(12, 1, '090012', 'Programacion I', 3, 5, 5, 80, TRUE, TRUE),
(13, 1, '090013', 'Calculo I', 3, 5, 5, 80, FALSE, TRUE),
(14, 1, '090014', 'Proceso Administrativo', 3, 4, 4, 64, FALSE, TRUE),
(15, 1, '090015', 'Derecho Informatico', 3, 5, 5, 80, FALSE, TRUE),
(16, 1, '090016', 'Microeconomia', 4, 5, 5, 80, FALSE, TRUE),
(17, 1, '090017', 'Programacion II', 4, 5, 5, 80, TRUE, TRUE),
(18, 1, '090018', 'Calculo II', 4, 5, 5, 80, FALSE, TRUE),
(19, 1, '090019', 'Estadistica I', 4, 5, 5, 80, FALSE, TRUE),
(20, 1, '090020', 'Fisica II', 4, 5, 5, 80, TRUE, TRUE),
(21, 1, '090021', 'Metodos Numericos', 5, 5, 5, 80, FALSE, TRUE),
(22, 1, '090022', 'Programacion III', 5, 5, 5, 80, TRUE, TRUE),
(23, 1, '090023', 'Emprendedores de Negocios', 5, 5, 5, 80, FALSE, TRUE),
(24, 1, '090024', 'Electronica Analogica', 5, 5, 5, 80, TRUE, TRUE),
(25, 1, '090025', 'Estadistica II', 5, 5, 5, 80, FALSE, TRUE),
(26, 1, '090026', 'Investigacion de Operaciones', 6, 5, 5, 80, FALSE, TRUE),
(27, 1, '090027', 'Bases de Datos I', 6, 5, 5, 80, TRUE, TRUE),
(28, 1, '090028', 'Automatas y Lenguajes Formales', 6, 5, 5, 80, FALSE, TRUE),
(29, 1, '090029', 'Sistemas Operativos I', 6, 5, 5, 80, TRUE, TRUE),
(30, 1, '090030', 'Electronica Digital', 6, 5, 5, 80, TRUE, TRUE),
(31, 1, '090031', 'Bases de Datos II', 7, 5, 5, 80, TRUE, TRUE),
(32, 1, '090032', 'Analisis de Sistemas I', 7, 5, 5, 80, FALSE, TRUE),
(33, 1, '090033', 'Sistemas Operativos II', 7, 5, 5, 80, TRUE, TRUE),
(34, 1, '090034', 'Arquitectura de Computadoras I', 7, 5, 5, 80, TRUE, TRUE),
(35, 1, '090035', 'Compiladores', 7, 5, 5, 80, FALSE, TRUE),
(36, 1, '090036', 'Desarrollo Web', 8, 5, 5, 80, TRUE, TRUE),
(37, 1, '090037', 'Analisis de Sistemas II', 8, 5, 5, 80, FALSE, TRUE),
(38, 1, '090038', 'Redes de Computadoras I', 8, 5, 5, 80, TRUE, TRUE),
(39, 1, '090039', 'Etica Profesional', 8, 4, 4, 64, FALSE, TRUE),
(40, 1, '090040', 'Arquitectura de Computadoras II', 8, 5, 5, 80, TRUE, TRUE),
(41, 1, '090041', 'Administracion de Tecnologias de Informacion', 9, 5, 5, 80, FALSE, TRUE),
(42, 1, '090042', 'Ingenieria de Software', 9, 5, 5, 80, FALSE, TRUE),
(43, 1, '090043', 'Proyecto de Graduacion I', 9, 6, 6, 96, FALSE, TRUE),
(44, 1, '090044', 'Redes de Computadoras II', 9, 5, 5, 80, TRUE, TRUE),
(45, 1, '090045', 'Inteligencia Artificial', 9, 5, 5, 80, FALSE, TRUE),
(46, 1, '090046', 'Telecomunicaciones', 10, 5, 5, 80, TRUE, TRUE),
(47, 1, '090047', 'Seminario de Tecnologias de Informacion', 10, 6, 6, 96, FALSE, TRUE),
(48, 1, '090048', 'Aseguramiento de la Calidad de Software', 10, 5, 5, 80, FALSE, TRUE),
(49, 1, '090049', 'Proyecto de Graduacion II', 10, 6, 6, 96, FALSE, TRUE),
(50, 1, '090050', 'Seguridad y Auditoria de Sistemas', 10, 5, 5, 80, TRUE, TRUE)
ON CONFLICT (id) DO UPDATE SET
    program_id = EXCLUDED.program_id,
    code = EXCLUDED.code,
    name = EXCLUDED.name,
    cycle = EXCLUDED.cycle,
    credits = EXCLUDED.credits,
    hours_per_week = EXCLUDED.hours_per_week,
    hours_total = EXCLUDED.hours_total,
    is_lab = EXCLUDED.is_lab,
    is_active = EXCLUDED.is_active;

-- Course prerequisites reset for Systems program
DELETE FROM course_prerequisites cp
USING courses c
WHERE cp.course_id = c.id
  AND c.program_id = 1;

DELETE FROM course_credit_requirements ccr
USING courses c
WHERE ccr.course_id = c.id
  AND c.program_id = 1;

INSERT INTO course_prerequisites (course_id, prerequisite_course_id) VALUES
(11, 6),
(12, 8),
(13, 6),
(17, 12),
(18, 13),
(20, 11),
(22, 17),
(24, 20),
(25, 19),
(27, 22),
(30, 24),
(31, 27),
(33, 29),
(35, 28),
(36, 31),
(37, 32),
(40, 34),
(44, 38),
(49, 43)
ON CONFLICT (course_id, prerequisite_course_id) DO NOTHING;

INSERT INTO course_credit_requirements (course_id, min_approved_credits) VALUES
(21, 70),
(26, 80),
(28, 80),
(29, 80),
(32, 100),
(34, 100),
(38, 125),
(39, 100),
(41, 150),
(42, 150),
(43, 150),
(45, 150),
(46, 175),
(47, 175),
(48, 175),
(50, 175)
ON CONFLICT DO NOTHING;

-- Course history
DELETE FROM student_course_history
WHERE student_id IN ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2');

INSERT INTO student_course_history (student_id, course_id, year, term, grade, status) VALUES
-- Ana (7mo ciclo con pendientes historicos)
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 1, 2022, '2022-1', 54.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 2, 2022, '2022-1', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 3, 2022, '2022-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 4, 2022, '2022-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 5, 2022, '2022-1', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 6, 2022, '2022-2', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 7, 2022, '2022-2', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 8, 2022, '2022-2', 87.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 9, 2022, '2022-2', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 10, 2022, '2022-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 11, 2023, '2023-1', 78.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 12, 2023, '2023-1', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 13, 2023, '2023-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 14, 2023, '2023-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 15, 2023, '2023-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 16, 2023, '2023-2', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 17, 2023, '2023-2', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 18, 2023, '2023-2', 77.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 19, 2023, '2023-2', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 20, 2023, '2023-2', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 21, 2024, '2024-1', 75.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 22, 2024, '2024-1', 56.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 23, 2024, '2024-1', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 24, 2024, '2024-1', 78.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 25, 2024, '2024-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 26, 2024, '2024-2', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 27, 2024, '2024-2', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 28, 2024, '2024-2', 77.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 29, 2024, '2024-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 30, 2024, '2024-2', 78.00, 'Passed'),
-- Carlos (al dia en 3er ciclo: ciclos 1 y 2 completos)
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 1, 2024, '2024-1', 78.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 2, 2024, '2024-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 3, 2024, '2024-1', 76.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 4, 2024, '2024-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 5, 2024, '2024-1', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 6, 2024, '2024-2', 77.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 7, 2024, '2024-2', 75.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 8, 2024, '2024-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 9, 2024, '2024-2', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 10, 2024, '2024-2', 78.00, 'Passed');

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
('11111111-1111-1111-1111-111111111111', 'SeedExecuted', 'Database', 'umg_seed_v2', '{"source":"seed.sql","institution":"UMG","currency":"GTQ"}'::jsonb, '127.0.0.1');
