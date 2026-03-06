# Arquitectura del Monorepo Academico UMG

## 1. Contexto
Solucion full-stack para gestion academica universitaria (Universidad Mariano Galvez de Guatemala) con enfoque DDD + Clean Architecture, autenticacion JWT por roles y persistencia PostgreSQL con DB-first.

## 2. Estructura
- `backend/src/Academic.Domain`: reglas y value objects de dominio.
- `backend/src/Academic.Application`: contratos, DTOs, CQRS (MediatR), `Result<T>`.
- `backend/src/Academic.Infrastructure`: EF Core Scaffold, adapters y servicios de negocio.
- `backend/src/Academic.Api`: controllers REST, seguridad, middleware global y Swagger.
- `frontend/academic-portal`: Angular 20 + Tailwind organizado por features.
- `db`: `schema.sql`, `seed.sql`, `data_fix_consistency.sql` como fuente de verdad DB-first.

## 3. Patrones aplicados
### DDD / Clean
- Dependencias hacia adentro (`Api -> Application -> Domain`, `Infrastructure -> Application + Domain`).
- Entidades scaffold separadas de la logica de negocio en servicios de infraestructura.

### CQRS
- Handlers MediatR por caso de uso (auth, traslados, asignacion, pagos, certificaciones, reportes).

### Resultado estandar
- `Result<T>` para respuestas consistentes y errores funcionales controlados.

## 4. DB-first y consistencia
- Diseño SQL en `db/schema.sql`.
- Seeds UMG y catalogo GTQ en `db/seed.sql`.
- Limpieza anti-inconsistencias y estados huerfanos en `db/data_fix_consistency.sql`.
- Regla del proyecto: no colocar logica de negocio dentro de clases scaffold.

## 5. Seguridad
- JWT firmado con clave simetrica.
- Roles `Student` y `Admin`.
- Hash de contrasenas con BCrypt (`pgcrypto` en seed + verificacion BCrypt.Net).
- Lista blanca de dominios institucionales configurable.
- Auto logout por inactividad en frontend.

## 6. Politica anti-bloqueo
Se aplica una regla operacional por modulo (`transfer`, `enrollment`, `certificate`):
- Maximo una solicitud activa por estudiante.
- `PendingPayment` expira automaticamente en 72 horas.
- Al expirar pago pendiente, se cancela la solicitud asociada.
- El estudiante puede cancelar solicitudes pendientes de pago manualmente.

## 7. Flujos clave
### Traslado de sede
- Consulta disponibilidad por sede/jornada.
- Crea solicitud + orden de pago en GTQ con expiracion.
- Admin revisa `PendingReview` (`Approved`/`Rejected`).
- Si se aprueba, se actualiza sede/jornada del estudiante y cupos de capacidad.

### Asignacion de cursos
- Detecta atrasados por historial (`Failed` sin `Passed` posterior).
- Valida prerequisitos y cupos.
- Crea asignacion + orden de pago en GTQ con expiracion.
- Permite cancelacion si esta pendiente de pago.

### Certificacion digital
- Solicitud genera orden en GTQ.
- Generacion PDF solo cuando pago esta en `Paid`.
- Codigo unico de verificacion y opcion de envio por email.
- Permite cancelacion si el estado es `Requested`.

## 8. Observabilidad y calidad
- Serilog para logging estructurado.
- Middleware global de excepciones.
- Swagger/OpenAPI con seguridad Bearer.
- Pruebas unitarias base en Domain/Application.

## 9. Trade-offs
- Se priorizo trazabilidad y coherencia de estado sobre simplicidad de flujo.
- La expiracion de pagos se implementa en capa de negocio para mantener control de reglas sin jobs externos obligatorios.
- Tarifas GTQ son referenciales y configurables en DB (no arancel contractual).
