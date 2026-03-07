# Arquitectura del Monorepo Académico UMG

## 1. Contexto
Solución full-stack para gestión académica de la Universidad Mariano Gálvez de Guatemala (UMG) con enfoque DDD + Clean Architecture, autenticación JWT por roles y persistencia PostgreSQL con **EF Core Code First**.

## 2. Estructura
- `backend/src/Academic.Domain`: reglas y value objects de dominio (`InstitutionalEmail`, `Carnet`).
- `backend/src/Academic.Application`: contratos, DTOs, CQRS (MediatR), `Result<T>`.
- `backend/src/Academic.Infrastructure`: entidades EF (`Persistence/Entities`), migraciones (`Persistence/Migrations`) y servicios de negocio.
- `backend/src/Academic.Api`: controllers REST, seguridad, middleware global y Swagger.
- `frontend/academic-portal`: Angular 20 + Tailwind por features.
- `db`: `schema.sql`, `seed.sql`, `data_fix_consistency.sql` y script idempotente.

## 3. Patrones aplicados
### DDD / Clean
- Dependencias hacia adentro (`Api -> Application -> Domain`, `Infrastructure -> Application + Domain`).
- La lógica de negocio vive en servicios de aplicación/infraestructura, no en controladores.

### CQRS
- Handlers MediatR por caso de uso (auth, traslados, asignación, pagos, certificaciones, reportes).

### Resultado estándar
- `Result<T>` para respuestas consistentes, errores funcionales y validaciones.

## 4. Persistencia Code First
- Modelo EF en `AcademicDbContext` + entidades tipadas.
- Migraciones versionadas en `Persistence/Migrations`.
- `db/schema.sql` como snapshot SQL legible del modelo.
- `db/schema.idempotent.sql` generado desde migraciones para despliegues incrementales.

## 5. Modelo académico UMG (Fase 1)
- Carrera 1: **Ingeniería en Sistemas de Información y Ciencias de la Computación**.
- Carrera 2: **Licenciatura en Psicología Industrial/Organizacional** (Pensum `7305`, año 2014).
- Pensum Sistemas 2014 completo (50 cursos, ciclos, créditos, lab, prerequisitos por curso y por créditos).
- Pensum Psicología Industrial 7305 (ciclos 1-10, cursos `100` a `143`, prerequisitos del documento oficial).
- Carnet real modelado: `NNNN-YY-NNNN|NNNNN`.
- Catálogo `carnet_prefix_catalog` para inferir sede/plan/carrera (incluye `0908` Escuintla sábado y `7305` Mazatenango sábado).
- Traslados con modalidad `Presencial` o `Virtual`.

## 6. Seguridad
- JWT firmado con clave simétrica.
- Roles `Student` y `Admin`.
- Hash de contraseñas con BCrypt.
- Lista blanca de dominios institucionales configurable.
- Auto logout por inactividad en frontend.

## 7. Política anti-bloqueo
- Máximo una solicitud activa por módulo (`transfer`, `enrollment`, `certificate`).
- `PendingPayment` expira automáticamente en 72 horas.
- Al expirar pago pendiente, se cancela solicitud asociada.
- El estudiante puede cancelar solicitudes pendientes de pago manualmente.
- `db/data_fix_consistency.sql` repara estados huérfanos y audita la ejecución.

## 8. Flujos clave
### Traslado de sede
- Consulta disponibilidad por sede/jornada.
- Crea solicitud + orden de pago GTQ con expiración.
- Incluye modalidad (`Presencial`/`Virtual`).
- Admin revisa `PendingReview` (`Approved`/`Rejected`).

### Asignación de cursos
- Detecta atrasados por historial (`Failed` sin `Passed` posterior).
- Valida prerequisitos por curso y por créditos aprobados.
- Crea asignación + orden de pago GTQ con expiración.

### Certificación digital
- Tipos implementados: cursos, matrícula, pasantías y cierre de pensum.
- Solicitud genera orden en GTQ.
- Regla crítica: `Cierre de pensum` solo si el estudiante tiene el 100% de cursos activos de su programa aprobados.
- PDF solo cuando pago está en `Paid`.
- Código único de verificación y envío opcional por email.

## 9. Observabilidad y calidad
- Serilog para logging estructurado.
- Middleware global de excepciones.
- Swagger/OpenAPI con seguridad Bearer.
- Pruebas unitarias base en Domain/Application.

## 10. Trade-offs
- Se priorizó consistencia y trazabilidad transaccional sobre simplicidad de modelo.
- Se usa una sola base para operación + catálogos para reducir complejidad en fase académica inicial.
- Tarifas en GTQ son referenciales y configurables en DB.
