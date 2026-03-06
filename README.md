# Proyecto 2 - Monorepo Academico UMG

Monorepo full-stack para gestion academica (Universidad Mariano Galvez de Guatemala) con backend .NET 8 + frontend Angular 20 + PostgreSQL DB-first.

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
|   |-- tools/
|   |   |-- DbBootstrapper/
|   |-- tests/
|-- frontend/
|   |-- academic-portal/
|-- db/
|   |-- schema.sql
|   |-- seed.sql
|   |-- data_fix_consistency.sql
|-- docs/
|   |-- architecture.md
|-- docker-compose.yml
|-- .env.example
```

## Variables de entorno backend
Archivo `backend/.env`:
```env
ConnectionStrings__Default=<NEON_CONNECTION_STRING>
Jwt__Key=SuperSecretDevelopmentKey_ChangeInProduction_123456789
Jwt__Issuer=Academic.Api
Jwt__Audience=Academic.Client
Jwt__ExpirationMinutes=120
Auth__AllowedEmailDomains__0=umg.edu.gt
Auth__AllowedEmailDomains__1=alumnos.umg.edu.gt
Auth__AllowedEmailDomains__2=universidad.edu
Auth__AllowedEmailDomains__3=alumnos.universidad.edu
Email__UseMock=true
Email__From=noreply@umg.edu.gt
Academic__DefaultCourseCapacity=40
Academic__PendingPaymentExpirationHours=72
Academic__DefaultCurrency=GTQ
```

> La connection string Neon tambien se mantiene en `backend/src/Academic.Api/appsettings.Development.json`.

## Base de datos (DB-first)
### 1) Aplicar schema + seed + limpieza de inconsistencias
Opcion recomendada:
```bash
cd backend
dotnet run --project tools/DbBootstrapper/DbBootstrapper.csproj -- "<NEON_CONNECTION_STRING>"
```

Este comando ejecuta en orden:
- `db/schema.sql`
- `db/seed.sql`
- `db/data_fix_consistency.sql`

### 2) Scaffold EF Core
```bash
dotnet ef dbcontext scaffold "<CONNECTION_STRING_NPGSQL>" Npgsql.EntityFrameworkCore.PostgreSQL -o Persistence/Scaffold -c AcademicDbContext -f --project src/Academic.Infrastructure/Academic.Infrastructure.csproj --startup-project src/Academic.Api/Academic.Api.csproj --no-onconfiguring
```

## Docker local
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
- `http://localhost:5262/swagger`

## Correr frontend
```bash
cd frontend/academic-portal
npm install
npm run start
```
App Angular:
- `http://localhost:4200`

## Troubleshooting (Windows - MSB3026/MSB3027)
Si aparece bloqueo de `Academic.Api.exe` o DLLs:
```powershell
Get-Process Academic.Api -ErrorAction SilentlyContinue | Stop-Process -Force
```
Luego vuelve a ejecutar `dotnet build` / `dotnet run`.

## Credenciales demo (seed)
- Admin:
  - `admin@umg.edu.gt`
  - `Admin123!`
- Student 1:
  - `ana.gomez@alumnos.umg.edu.gt`
  - `Student123!`
- Student 2:
  - `juan.perez@alumnos.umg.edu.gt`
  - `Student123!`

## Tarifas referenciales (GTQ)
- Transfer: `Q150.00`
- Enrollment base: `Q60.00`
- CourseExtra: `Q175.00`
- CourseOverdue: `Q130.00`
- Certificate: `Q70.00`

## Flujos anti-bloqueo implementados
- Una solicitud activa por modulo (traslado/asignacion/certificacion).
- Expiracion automatica de pagos pendientes (`72h`) con cancelacion relacionada.
- Cancelacion explicita para solicitudes pendientes de pago.
- Limpieza de estados huerfanos con `db/data_fix_consistency.sql`.

## Endpoints principales
- Auth:
  - `POST /auth/login`
  - `GET /me`
- Campuses:
  - `GET /campuses`
- Transfers:
  - `GET /transfers/availability?campusId=&shift=`
  - `POST /transfers`
  - `GET /transfers/my`
  - `POST /transfers/{id}/cancel`
  - `POST /transfers/{id}/review` (Admin)
- Courses / Enrollments:
  - `GET /courses/pensum`
  - `GET /courses/overdue`
  - `POST /enrollments`
  - `GET /enrollments/my`
  - `POST /enrollments/{id}/cancel`
- Payments:
  - `GET /payments/my` (Student)
  - `GET /payments/pending` (Admin)
  - `POST /payments/{id}/mark-paid` (Admin)
- Certificates:
  - `POST /certificates`
  - `GET /certificates/my`
  - `POST /certificates/{id}/cancel`
  - `POST /certificates/{id}/generate`
  - `GET /certificates/{id}/download`
  - `GET /certificates/verify/{code}`
- Reports:
  - `GET /reports/transfers`
  - `GET /reports/enrollments`
  - `GET /reports/certificates`
  - `GET /reports/{reportType}/export?format=pdf|xlsx`

## Pruebas
```bash
cd backend
dotnet test Academic.sln
```

```bash
cd frontend/academic-portal
npm run lint
npm run build
```

## Arquitectura
Ver detalle en `docs/architecture.md`.
