# Arquitectura del Monorepo Academico

## 1. Contexto
Solucion full-stack para gestion academica con enfoque DDD + Clean Architecture, autenticacion JWT por roles y persistencia PostgreSQL Database-First.

## 2. Estructura
- `backend/src/Academic.Domain`: reglas y objetos de dominio.
- `backend/src/Academic.Application`: contratos, DTOs, CQRS (MediatR), `Result<T>`.
- `backend/src/Academic.Infrastructure`: EF Core Scaffold, adapters, servicios de negocio, JWT, PDF, email, auditoria.
- `backend/src/Academic.Api`: controladores REST, seguridad, Swagger, middleware de errores.
- `frontend/academic-portal`: Angular 20 + Tailwind por features.
- `db`: `schema.sql` y `seed.sql` como fuente de verdad DB-first.

## 3. Patrones aplicados
### DDD / Clean
- Dependencias dirigidas hacia adentro (`Api -> Application -> Domain`, `Infrastructure -> Application + Domain`).
- Entidades scaffold separadas de reglas de negocio mediante servicios/adapters.

### CQRS
- Handlers MediatR por caso de uso (login, traslado, asignacion, pagos, certificaciones, reportes).

### Resultado estandar
- `Result<T>` para respuestas consistentes y manejo de errores funcionales.

## 4. Database-First
- Diseño en `db/schema.sql` con constraints, indices y estados controlados.
- Seed en `db/seed.sql` para demo inmediata.
- Reverse engineering EF Core a `Persistence/Scaffold`.
- Regla de proyecto: no incorporar logica de negocio dentro de clases scaffold.

## 5. Seguridad
- JWT firmado con clave simetrica.
- Roles `Student` y `Admin`.
- Hash de contrasenas con BCrypt (hashes sembrados por `pgcrypto` en seed).
- Lista blanca de dominios institucionales configurable (`Auth__AllowedEmailDomains`).
- Auto logout por inactividad en frontend.

## 6. Modulos
### Traslado de sede
- Consulta sedes y disponibilidad por jornada.
- Validacion de cupos y duplicidad de solicitud activa.
- Creacion automatica de orden de pago.

### Asignacion de cursos
- Pensum por estudiante.
- Deteccion de cursos atrasados (Failed sin Passed posterior).
- Validacion de prerequisitos y cupos logicos por capacidad configurable.
- Orden de pago automatica.

### Certificacion digital
- Solicitud crea orden de pago.
- Generacion PDF solo cuando pago esta en `Paid`.
- Codigo unico de verificacion.
- Descarga y envio por email (SMTP o mock).

### Reportes admin
- Reportes de traslados, asignaciones y certificaciones.
- Export PDF (QuestPDF) y Excel (ClosedXML).

## 7. Observabilidad y calidad
- Serilog para logging estructurado.
- Middleware global de excepciones.
- Swagger/OpenAPI con seguridad Bearer.
- Pruebas unitarias basicas en Domain/Application.

## 8. Trade-offs
- Se priorizo una implementacion de referencia ejecutable y didactica para entorno universitario.
- Repositorios por agregado se encapsulan en servicios de negocio por modulo para mantener simpleza del prototipo.
- Validacion de cupos de cursos se controla por capacidad configurable en app (`Academic:DefaultCourseCapacity`) usando conteo de inscripciones confirmadas.
