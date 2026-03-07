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
(1, 'SIS-2014', 'Ingenieria en Sistemas de Informacion y Ciencias de la Computacion', TRUE),
(2, 'PSI-7305-2014', 'Licenciatura en Psicologia Industrial/Organizacional', TRUE)
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
(18, 'UMG-SLUCIA', 'Centro Universitario Santa Lucia Cotzumalguapa', '6a Calle 2-30, Zona 1, Santa Lucia Cotzumalguapa', 'Centro', 'Escuintla', TRUE),
(19, 'UMG-MAZA', 'Centro Universitario Mazatenango', '4a Calle 8-40, Mazatenango, Suchitepequez', 'Centro', 'Suchitepequez', TRUE)
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
(18, 1, 110, 42), (18, 2, 110, 39),
(19, 1, 120, 44), (19, 2, 120, 41)
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
('0910', 1, 16, 1, 'Sistemas Centro Chimaltenango plan sabado', TRUE),
('7305', 2, 19, 1, 'Psicologia Industrial Mazatenango plan sabado', TRUE),
('7306', 2, 19, 2, 'Psicologia Industrial Mazatenango plan domingo', TRUE)
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
('33333333-3333-3333-3333-333333333333', 'carlos.salazar@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE),
('44444444-4444-4444-4444-444444444444', 'maria.ortiz@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE),
('55555555-5555-5555-5555-555555555555', 'jorge.castillo@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE),
('66666666-6666-6666-6666-666666666666', 'paola.ramirez@alumnos.umg.edu.gt', crypt('Student123!', gen_salt('bf')), TRUE)
ON CONFLICT (id) DO UPDATE SET
    email = EXCLUDED.email,
    password_hash = EXCLUDED.password_hash,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Roles for users
INSERT INTO user_roles (user_id, role_id) VALUES
('11111111-1111-1111-1111-111111111111', 1),
('22222222-2222-2222-2222-222222222222', 2),
('33333333-3333-3333-3333-333333333333', 2),
('44444444-4444-4444-4444-444444444444', 2),
('55555555-5555-5555-5555-555555555555', 2),
('66666666-6666-6666-6666-666666666666', 2)
ON CONFLICT (user_id, role_id) DO NOTHING;

-- Reset transactional demo data to keep seed idempotent and avoid stale locks.
DELETE FROM enrollment_courses
WHERE enrollment_id IN (
    SELECT id
    FROM enrollments
    WHERE student_id IN (
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
        'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
    )
);

DELETE FROM certificates
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
);

DELETE FROM transfer_requests
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
);

DELETE FROM enrollments
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
);

DELETE FROM payment_orders
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
);

DELETE FROM audit_logs
WHERE user_id IN (
    '22222222-2222-2222-2222-222222222222',
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444',
    '55555555-5555-5555-5555-555555555555',
    '66666666-6666-6666-6666-666666666666'
);

-- Students
INSERT INTO students (
    id, user_id, student_code, carnet, carnet_prefix, entry_year, carnet_sequence,
    institutional_email, first_name, last_name, program_id, current_campus_id, current_shift_id, is_active
) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-2222-2222-2222-222222222222', 'SIS-22001', '0908-22-14264', '0908', 22, '14264', 'ana.gomez@alumnos.umg.edu.gt', 'Ana Lucia', 'Gomez Morales', 1, 17, 1, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '33333333-3333-3333-3333-333333333333', 'SIS-23009', '0909-23-09876', '0909', 23, '09876', 'carlos.salazar@alumnos.umg.edu.gt', 'Carlos Eduardo', 'Salazar Lopez', 1, 17, 2, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '44444444-4444-4444-4444-444444444444', 'PSI-21001', '7305-21-10458', '7305', 21, '10458', 'maria.ortiz@alumnos.umg.edu.gt', 'Maria Fernanda', 'Ortiz Lopez', 2, 19, 1, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', '55555555-5555-5555-5555-555555555555', 'SIS-24015', '0904-24-15321', '0904', 24, '15321', 'jorge.castillo@alumnos.umg.edu.gt', 'Jorge Andres', 'Castillo Rivera', 1, 5, 1, TRUE),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', '66666666-6666-6666-6666-666666666666', 'PSI-24007', '7306-24-11234', '7306', 24, '11234', 'paola.ramirez@alumnos.umg.edu.gt', 'Paola Andrea', 'Ramirez Soto', 2, 19, 2, TRUE)
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

-- Courses (Pensum Psicologia Industrial/Organizacional 7305 - 2014)
INSERT INTO courses (id, program_id, code, name, cycle, credits, hours_per_week, hours_total, is_lab, is_active) VALUES
(100, 2, '100', 'Filosofia', 1, 5, 5, 80, FALSE, TRUE),
(101, 2, '101', 'Desarrollo Humano y Profesional', 1, 5, 5, 80, FALSE, TRUE),
(102, 2, '102', 'Biologia Humana', 1, 5, 5, 80, FALSE, TRUE),
(103, 2, '103', 'Sociologia General', 1, 5, 5, 80, FALSE, TRUE),
(104, 2, '104', 'Antropologia General', 2, 5, 5, 80, FALSE, TRUE),
(105, 2, '105', 'Logica Formal', 2, 5, 5, 80, FALSE, TRUE),
(106, 2, '106', 'Psicologia General', 2, 5, 5, 80, FALSE, TRUE),
(107, 2, '107', 'Metodologia de la Investigacion', 2, 5, 5, 80, FALSE, TRUE),
(108, 2, '108', 'Anatomia y Fisiologia del Sistema Nervioso', 3, 5, 5, 80, FALSE, TRUE),
(109, 2, '109', 'Estadistica Fundamental', 3, 5, 5, 80, FALSE, TRUE),
(110, 2, '110', 'Psicologia Evolutiva del Nino y del Adolescente', 3, 5, 5, 80, FALSE, TRUE),
(111, 2, '111', 'Semiologia Psicologica', 3, 5, 5, 80, FALSE, TRUE),
(112, 2, '112', 'Psicometria I', 4, 5, 5, 80, FALSE, TRUE),
(113, 2, '113', 'Teorias de la Personalidad', 4, 5, 5, 80, FALSE, TRUE),
(114, 2, '114', 'Estadistica Aplicada a la Psicologia', 4, 5, 5, 80, FALSE, TRUE),
(115, 2, '115', 'Psicologia Evolutiva del Adulto', 4, 5, 5, 80, FALSE, TRUE),
(116, 2, '116', 'Psicometria II', 5, 5, 5, 80, FALSE, TRUE),
(117, 2, '117', 'Psicologia del Deporte y la Recreacion', 5, 5, 5, 80, FALSE, TRUE),
(118, 2, '118', 'Psicologia Social', 5, 5, 5, 80, FALSE, TRUE),
(119, 2, '119', 'Neurofisiologia', 5, 5, 5, 80, FALSE, TRUE),
(120, 2, '120', 'Psicologia Clinica', 6, 5, 5, 80, FALSE, TRUE),
(121, 2, '121', 'Introduccion a la Psicologia Forense', 6, 5, 5, 80, FALSE, TRUE),
(122, 2, '122', 'Introduccion a la Psicologia Industrial/Organizacional', 6, 5, 5, 80, FALSE, TRUE),
(123, 2, '123', 'Fundamentos de Informatica', 6, 5, 5, 80, FALSE, TRUE),
(124, 2, '124', 'Teoria Administrativa', 7, 5, 5, 80, FALSE, TRUE),
(125, 2, '125', 'Competencias Laborales', 7, 5, 5, 80, FALSE, TRUE),
(126, 2, '126', 'Planeacion Estrategica de Recursos Humanos', 7, 5, 5, 80, FALSE, TRUE),
(127, 2, '127', 'Legislacion Laboral', 7, 5, 5, 80, FALSE, TRUE),
(128, 2, '128', 'Comportamiento Organizacional', 7, 5, 5, 80, FALSE, TRUE),
(129, 2, '129', 'Analisis y Valuacion de Puestos', 8, 5, 5, 80, FALSE, TRUE),
(130, 2, '130', 'Desarrollo Organizacional', 8, 5, 5, 80, FALSE, TRUE),
(131, 2, '131', 'Elaboracion de Proyectos', 8, 5, 5, 80, FALSE, TRUE),
(132, 2, '132', 'Psicometria Laboral', 8, 5, 5, 80, FALSE, TRUE),
(133, 2, '133', 'Provision del Talento Humano', 8, 5, 5, 80, FALSE, TRUE),
(134, 2, '134', 'Desarrollo del Talento Humano', 9, 5, 5, 80, FALSE, TRUE),
(135, 2, '135', 'Gestion del Desempeno', 9, 5, 5, 80, FALSE, TRUE),
(136, 2, '136', 'Gestion de Proyectos', 9, 5, 5, 80, FALSE, TRUE),
(137, 2, '137', 'Practica Supervisada I', 9, 5, 5, 80, FALSE, TRUE),
(138, 2, '138', 'Elaboracion de Trabajo de Graduacion I', 9, 5, 5, 80, FALSE, TRUE),
(139, 2, '139', 'Gestion de la Compensacion', 10, 5, 5, 80, FALSE, TRUE),
(140, 2, '140', 'Gestion de Indicadores Laborales', 10, 5, 5, 80, FALSE, TRUE),
(141, 2, '141', 'Seguridad Industrial y Salud Ocupacional', 10, 5, 5, 80, FALSE, TRUE),
(142, 2, '142', 'Practica Supervisada II', 10, 5, 5, 80, FALSE, TRUE),
(143, 2, '143', 'Elaboracion de Trabajo de Graduacion II', 10, 5, 5, 80, FALSE, TRUE)
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

-- Course prerequisites reset for seeded programs
DELETE FROM course_prerequisites cp
USING courses c
WHERE cp.course_id = c.id
  AND c.program_id IN (1, 2);

DELETE FROM course_credit_requirements ccr
USING courses c
WHERE ccr.course_id = c.id
  AND c.program_id IN (1, 2);

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

INSERT INTO course_prerequisites (course_id, prerequisite_course_id) VALUES
(108, 102),
(109, 107),
(114, 109),
(115, 110),
(116, 112),
(119, 108),
(120, 118),
(124, 122),
(125, 122),
(126, 122),
(127, 122),
(128, 118),
(129, 126),
(130, 128),
(132, 116),
(133, 126),
(134, 129),
(135, 129),
(136, 131),
(137, 132),
(138, 114),
(139, 135),
(140, 137),
(141, 127),
(142, 137),
(143, 138)
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
WHERE student_id IN (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5'
);

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
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 10, 2024, '2024-2', 78.00, 'Passed'),
-- Maria (pensum completo Psicologia Industrial 7305 aprobado)
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 100, 2021, '2021-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 101, 2021, '2021-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 102, 2021, '2021-1', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 103, 2021, '2021-1', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 104, 2021, '2021-2', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 105, 2021, '2021-2', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 106, 2021, '2021-2', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 107, 2021, '2021-2', 89.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 108, 2022, '2022-1', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 109, 2022, '2022-1', 87.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 110, 2022, '2022-1', 91.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 111, 2022, '2022-1', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 112, 2022, '2022-2', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 113, 2022, '2022-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 114, 2022, '2022-2', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 115, 2022, '2022-2', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 116, 2023, '2023-1', 89.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 117, 2023, '2023-1', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 118, 2023, '2023-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 119, 2023, '2023-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 120, 2023, '2023-2', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 121, 2023, '2023-2', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 122, 2023, '2023-2', 91.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 123, 2023, '2023-2', 87.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 124, 2024, '2024-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 125, 2024, '2024-1', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 126, 2024, '2024-1', 89.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 127, 2024, '2024-1', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 128, 2024, '2024-1', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 129, 2024, '2024-2', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 130, 2024, '2024-2', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 131, 2024, '2024-2', 87.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 132, 2024, '2024-2', 89.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 133, 2024, '2024-2', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 134, 2025, '2025-1', 91.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 135, 2025, '2025-1', 92.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 136, 2025, '2025-1', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 137, 2025, '2025-1', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 138, 2025, '2025-1', 87.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 139, 2025, '2025-2', 89.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 140, 2025, '2025-2', 91.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 141, 2025, '2025-2', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 142, 2025, '2025-2', 90.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 143, 2025, '2025-2', 93.00, 'Passed'),
-- Jorge (Sistemas, 5to ciclo con 2 atrasados: 090001 y 090017)
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 1, 2024, '2024-1', 58.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 2, 2024, '2024-1', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 3, 2024, '2024-1', 77.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 4, 2024, '2024-1', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 5, 2024, '2024-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 6, 2024, '2024-2', 76.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 7, 2024, '2024-2', 78.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 8, 2024, '2024-2', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 9, 2024, '2024-2', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 10, 2024, '2024-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 11, 2025, '2025-1', 75.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 12, 2025, '2025-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 13, 2025, '2025-1', 77.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 14, 2025, '2025-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 15, 2025, '2025-1', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 16, 2025, '2025-2', 74.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 17, 2025, '2025-2', 55.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 18, 2025, '2025-2', 76.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 19, 2025, '2025-2', 78.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 20, 2025, '2025-2', 75.00, 'Passed'),
-- Paola (Psicologia Industrial, 6to ciclo con atrasados 117 y 120)
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 100, 2024, '2024-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 101, 2024, '2024-1', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 102, 2024, '2024-1', 79.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 103, 2024, '2024-1', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 104, 2024, '2024-2', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 105, 2024, '2024-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 106, 2024, '2024-2', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 107, 2024, '2024-2', 88.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 108, 2025, '2025-1', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 109, 2025, '2025-1', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 110, 2025, '2025-1', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 111, 2025, '2025-1', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 112, 2025, '2025-2', 87.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 113, 2025, '2025-2', 84.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 114, 2025, '2025-2', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 115, 2025, '2025-2', 86.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 116, 2026, '2026-1', 83.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 117, 2026, '2026-1', 57.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 118, 2026, '2026-1', 81.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 119, 2026, '2026-1', 80.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 120, 2026, '2026-2', 58.00, 'Failed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 121, 2026, '2026-2', 82.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 122, 2026, '2026-2', 85.00, 'Passed'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 123, 2026, '2026-2', 83.00, 'Passed');

-- Pricing catalog in GTQ
UPDATE pricing_catalog
SET is_active = FALSE
WHERE service_type IN ('Transfer', 'Enrollment', 'CourseExtra', 'CourseOverdue', 'Certificate')
  AND is_active = TRUE;

INSERT INTO pricing_catalog (service_type, program_id, amount, currency, is_active) VALUES
('Transfer', NULL, 150.00, 'GTQ', TRUE),
('Enrollment', 1, 60.00, 'GTQ', TRUE),
('Enrollment', 2, 60.00, 'GTQ', TRUE),
('CourseExtra', 1, 175.00, 'GTQ', TRUE),
('CourseExtra', 2, 175.00, 'GTQ', TRUE),
('CourseOverdue', 1, 130.00, 'GTQ', TRUE),
('CourseOverdue', 2, 130.00, 'GTQ', TRUE),
('Certificate', NULL, 70.00, 'GTQ', TRUE)
ON CONFLICT DO NOTHING;

-- Demo certificates by type
INSERT INTO payment_orders (id, student_id, order_type, reference_id, amount, currency, status, description, created_at, expires_at, paid_at) VALUES
('70000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Certificate', '80000000-0000-0000-0000-000000000001', 70.00, 'GTQ', 'Paid', 'Pago Certificacion de cursos', NOW() - INTERVAL '12 days', NOW() - INTERVAL '10 days', NOW() - INTERVAL '11 days'),
('70000000-0000-0000-0000-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Certificate', '80000000-0000-0000-0000-000000000002', 70.00, 'GTQ', 'Paid', 'Pago Certificacion de matricula', NOW() - INTERVAL '9 days', NOW() - INTERVAL '7 days', NOW() - INTERVAL '8 days'),
('70000000-0000-0000-0000-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Certificate', '80000000-0000-0000-0000-000000000003', 70.00, 'GTQ', 'Paid', 'Pago Cierre de pensum', NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days', NOW() - INTERVAL '4 days'),
('70000000-0000-0000-0000-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 'Certificate', '80000000-0000-0000-0000-000000000004', 70.00, 'GTQ', 'Paid', 'Pago Certificacion de cursos', NOW() - INTERVAL '4 days', NOW() - INTERVAL '2 days', NOW() - INTERVAL '3 days'),
('70000000-0000-0000-0000-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', 'Certificate', '80000000-0000-0000-0000-000000000005', 70.00, 'GTQ', 'Paid', 'Pago Certificacion de matricula', NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 days', NOW() - INTERVAL '2 days')
ON CONFLICT (id) DO UPDATE SET
    status = EXCLUDED.status,
    description = EXCLUDED.description,
    amount = EXCLUDED.amount,
    currency = EXCLUDED.currency,
    paid_at = EXCLUDED.paid_at;

INSERT INTO certificates (id, student_id, payment_order_id, purpose, status, verification_code, pdf_path, metadata, created_at, generated_at, sent_at) VALUES
('80000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '70000000-0000-0000-0000-000000000001', 'Certificacion de cursos', 'Sent', 'UMG-CERT-COURSES-0001', NULL, '{"certificateType":"courses","source":"seed.sql"}'::jsonb, NOW() - INTERVAL '12 days', NOW() - INTERVAL '11 days', NOW() - INTERVAL '11 days'),
('80000000-0000-0000-0000-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '70000000-0000-0000-0000-000000000002', 'Certificacion de matricula', 'Sent', 'UMG-CERT-ENROLL-0002', NULL, '{"certificateType":"enrollment","source":"seed.sql"}'::jsonb, NOW() - INTERVAL '9 days', NOW() - INTERVAL '8 days', NOW() - INTERVAL '8 days'),
('80000000-0000-0000-0000-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '70000000-0000-0000-0000-000000000003', 'Cierre de pensum', 'Sent', 'UMG-CERT-PENSUM-0003', NULL, '{"certificateType":"pensum-closure","source":"seed.sql"}'::jsonb, NOW() - INTERVAL '5 days', NOW() - INTERVAL '4 days', NOW() - INTERVAL '4 days'),
('80000000-0000-0000-0000-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4', '70000000-0000-0000-0000-000000000004', 'Certificacion de cursos', 'Sent', 'UMG-CERT-COURSES-0004', NULL, '{"certificateType":"courses","source":"seed.sql"}'::jsonb, NOW() - INTERVAL '4 days', NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days'),
('80000000-0000-0000-0000-000000000005', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5', '70000000-0000-0000-0000-000000000005', 'Certificacion de matricula', 'Sent', 'UMG-CERT-ENROLL-0005', NULL, '{"certificateType":"enrollment","source":"seed.sql"}'::jsonb, NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days')
ON CONFLICT (id) DO UPDATE SET
    status = EXCLUDED.status,
    purpose = EXCLUDED.purpose,
    metadata = EXCLUDED.metadata,
    generated_at = EXCLUDED.generated_at,
    sent_at = EXCLUDED.sent_at;

-- Audit seed execution
INSERT INTO audit_logs (user_id, action, entity_name, entity_id, details, ip_address)
VALUES
('11111111-1111-1111-1111-111111111111', 'SeedExecuted', 'Database', 'umg_seed_v2', '{"source":"seed.sql","institution":"UMG","currency":"GTQ"}'::jsonb, '127.0.0.1');
