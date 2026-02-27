# Proyecto 2 - Monorepo Academico Full-Stack

Monorepo full-stack para gestion academica universitaria.

## Stack
- Backend: ASP.NET Core Web API (.NET 8), DDD + Clean Architecture + CQRS (MediatR)
- ORM: EF Core (Database-First) con PostgreSQL
- DB: Neon (cloud) + opcion local con Docker
- Frontend: Angular 20.3.x (LTS) + TailwindCSS
- Auth: JWT + roles (`Student`, `Admin`)

## Estructura
```text
/
|-- backend/
|   |-- Academic.sln
|   |-- .env
|   |-- src/
|   |   |-- Academic.Domain/
|   |   |-- Academic.Application/
|   |   |-- Academic.Infrastructure/
|   |   |   |-- Persistence/Scaffold/
|   |   |-- Academic.Api/
|   |-- tests/
|-- frontend/
|   |-- academic-portal/
|-- db/
|   |-- schema.sql
|   |-- seed.sql
|-- docs/
|   |-- architecture.md
|-- docker-compose.yml
|-- .env.example
```

## Variables de entorno
En `backend/.env`:
```env
ConnectionStrings__Default=postgresql://neondb_owner:npg_1jbTQGg9ziIh@ep-fragrant-frog-aivdvzj3-pooler.c-4.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require
Jwt__Key=SuperSecretDevelopmentKey_ChangeInProduction_123456789
Jwt__Issuer=Academic.Api
Jwt__Audience=Academic.Client
Auth__AllowedEmailDomains__0=universidad.edu
Auth__AllowedEmailDomains__1=alumnos.universidad.edu
Email__UseMock=true
```

> Tambien se incluye esta connection string en `backend/src/Academic.Api/appsettings.Development.json`.

## Base de datos (Neon / local)
### 1) Ejecutar schema y seed
- Fuente de verdad: `db/schema.sql`
- Datos demo: `db/seed.sql`

Se puede ejecutar con cualquier cliente PostgreSQL (Neon SQL Editor, DBeaver, psql, etc.).

### 2) Scaffold EF Core (Database-First)
Comando estandar:
```bash
dotnet ef dbcontext scaffold "<NEON_CONNECTION_STRING>" Npgsql.EntityFrameworkCore.PostgreSQL -o Persistence/Scaffold -c AcademicDbContext -f
```

Comando usado en este repositorio (desde `backend`):
```bash
dotnet ef dbcontext scaffold "Host=ep-fragrant-frog-aivdvzj3-pooler.c-4.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_1jbTQGg9ziIh;SSL Mode=Require;Channel Binding=Require" Npgsql.EntityFrameworkCore.PostgreSQL -o Persistence/Scaffold -c AcademicDbContext -f --project src/Academic.Infrastructure/Academic.Infrastructure.csproj --no-onconfiguring
```

### 3) Opcion local con Docker
```bash
docker compose up -d
```
Servicios:
- PostgreSQL: `localhost:5432`
- pgAdmin: `http://localhost:5050` (`admin@local.dev` / `admin`)

## Correr backend
```bash
cd backend
dotnet restore
dotnet build Academic.sln
dotnet run --project src/Academic.Api/Academic.Api.csproj
```
Swagger:
- `http://localhost:5000/swagger`

## Correr frontend
```bash
cd frontend/academic-portal
npm install
npm run start
```
App Angular:
- `http://localhost:4200`

## Pruebas
```bash
cd backend
dotnet test Academic.sln
```

## Credenciales demo (seed)
- Admin:
  - `admin@universidad.edu`
  - `Admin123!`
- Student 1:
  - `ana.gomez@alumnos.universidad.edu`
  - `Student123!`
- Student 2:
  - `juan.perez@alumnos.universidad.edu`
  - `Student123!`

## Endpoints principales
- `POST /auth/login`
- `GET /me`
- `GET /campuses`
- `GET /transfers/availability?campusId=&shift=`
- `POST /transfers`
- `GET /transfers/my`
- `GET /courses/pensum`
- `GET /courses/overdue`
- `POST /enrollments`
- `GET /payments/my`
- `POST /payments/{id}/mark-paid`
- `POST /certificates`
- `POST /certificates/{id}/generate`
- `GET /certificates/{id}/download`
- `GET /certificates/verify/{code}`
- `GET /reports/transfers`
- `GET /reports/enrollments`
- `GET /reports/certificates`
- `GET /reports/{reportType}/export?format=pdf|xlsx`

## Ejemplos curl
### Login
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"ana.gomez@alumnos.universidad.edu","password":"Student123!"}'
```

### Crear traslado
```bash
curl -X POST http://localhost:5000/transfers \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"campusId":2,"shift":"Saturday","reason":"Cambio de ciudad"}'
```

### Crear asignacion de cursos
```bash
curl -X POST http://localhost:5000/enrollments \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{"courseIds":[3,5]}'
```

### Marcar pago como pagado (Admin)
```bash
curl -X POST http://localhost:5000/payments/<PAYMENT_ID>/mark-paid \
  -H "Authorization: Bearer <ADMIN_TOKEN>"
```

## Notas de migracion Neon/local
- Este proyecto es DB-first; no depende de migraciones code-first.
- Para cambiar de Neon a local, actualiza solo `ConnectionStrings__Default`.
- Si cambias el modelo SQL, vuelve a ejecutar scaffold en `Academic.Infrastructure/Persistence/Scaffold`.

## Calidad incluida
- Swagger con JWT bearer
- Manejo global de errores
- Logging con Serilog
- Hash BCrypt
- Bitacora (`audit_logs`)
- Pruebas unitarias basicas

## Arquitectura
Ver detalle de decisiones en:
- `docs/architecture.md`
