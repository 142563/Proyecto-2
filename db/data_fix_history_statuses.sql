BEGIN;

-- Normaliza estados historicos para evitar bloqueos de prerequisitos/aprobados
UPDATE student_course_history
SET status = 'Passed'
WHERE lower(btrim(status)) IN (
    'passed',
    'approved',
    'aprobado',
    'aprobada',
    'ganado',
    'ganada',
    'completed',
    'completado',
    'completada',
    'exonerado'
)
AND status <> 'Passed';

UPDATE student_course_history
SET status = 'Failed'
WHERE lower(btrim(status)) IN (
    'failed',
    'reprobado',
    'reprobada',
    'desaprobado',
    'desaprobada',
    'perdido',
    'perdida',
    'no_aprobado',
    'no aprobado',
    'unapproved'
)
AND status <> 'Failed';

COMMIT;
