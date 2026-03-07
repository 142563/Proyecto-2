# Proyecto 2 - Portal MIUMG Estudiantil

Monorepo full-stack para gestión académica de la **Universidad Mariano Gálvez de Guatemala (UMG)** con backend .NET 8, frontend Angular 20 y PostgreSQL.

## Stack
- Backend: ASP.NET Core Web API (.NET 8), DDD + Clean Architecture + CQRS (MediatR)
- ORM: EF Core **Code First** con migraciones
- DB: Neon (cloud) + opción local con Docker
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
|   |   |   |-- Persistence/Entities/
|   |   |   |-- Persistence/Migrations/
|   |   |-- Academic.Api/
|   |-- tools/
|   |   |-- DbBootstrapper/
|   |-- tests/
|-- frontend/
|   |-- academic-portal/
|-- db/
|   |-- schema.sql
|   |-- schema.idempotent.sql
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

> La connection string de Neon también se mantiene en `backend/src/Academic.Api/appsettings.Development.json`.

## Base de datos (Code First)
### 1) Aplicar migraciones
```bash
cd backend
dotnet ef database update --project src/Academic.Infrastructure/Academic.Infrastructure.csproj --startup-project src/Academic.Api/Academic.Api.csproj
```

### 2) Ejecutar seed + fixes de consistencia
```bash
dotnet run --project tools/DbBootstrapper/DbBootstrapper.csproj -- "<NEON_CONNECTION_STRING>"
```
Este comando aplica en orden:
- `db/schema.sql`
- `db/seed.sql`
- `db/data_fix_consistency.sql`

### 3) Crear nueva migración (cuando cambie el modelo)
```bash
dotnet ef migrations add <NombreMigracion> --project src/Academic.Infrastructure/Academic.Infrastructure.csproj --startup-project src/Academic.Api/Academic.Api.csproj --context AcademicDbContext -o Persistence/Migrations
```

### 4) Generar script idempotente
```bash
dotnet ef migrations script --idempotent --project src/Academic.Infrastructure/Academic.Infrastructure.csproj --startup-project src/Academic.Api/Academic.Api.csproj --context AcademicDbContext -o ../db/schema.idempotent.sql
```

## Docker local
```bash
docker compose up -d
```
Servicios:
- PostgreSQL: `localhost:5432`
- pgAdmin: `http://localhost:5050` (`admin@local.dev` / `admin`)

## Correr backend (puerto 5262)
```bash
cd backend
dotnet restore
dotnet build Academic.sln
dotnet run --project src/Academic.Api/Academic.Api.csproj
```
Swagger:
- `http://localhost:5262/swagger`

## Correr frontend (puerto 4200)
```bash
cd frontend/academic-portal
npm install
npm run start
```
App Angular:
- `http://localhost:4200`

## Troubleshooting (Windows - MSB3026/MSB3027)
Si aparece bloqueo de `Academic.Api` o DLLs:
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
  - Carnet: `0908-22-14264` (Escuintla, plan sábado)
- Student 2:
  - `carlos.salazar@alumnos.umg.edu.gt`
  - `Student123!`
  - Carnet: `0909-23-09876`
- Student 3 (Psicología Industrial 7305 - cierre de pensum elegible):
  - `maria.ortiz@alumnos.umg.edu.gt`
  - `Student123!`
  - Carnet: `7305-21-10458`

## Certificaciones UMG implementadas
- `Certificación de cursos`
- `Certificación de matrícula`
- `Certificación de pasantías`
- `Cierre de pensum` (solo si todo el pensum está aprobado)

Fuente pública UMG usada para el catálogo:
- https://miumg.umg.edu.gt/pensum (opción de certificaciones)
- https://www.umg.edu.gt/info/Paginas/Cierre-de-Pensum

## Tarifas referenciales (GTQ)
- Transfer: `Q150.00`
- Enrollment base: `Q60.00`
- CourseExtra: `Q175.00`
- CourseOverdue: `Q130.00`
- Certificate: `Q70.00`

## Formato de carnet
Patrón: `NNNN-YY-NNNN|NNNNN`
- `NNNN`: prefijo (sede/plan/carrera según `carnet_prefix_catalog`)
- `YY`: año de ingreso (2 dígitos)
- `NNNN|NNNNN`: correlativo de carnet

## Endpoints principales
- Auth:
  - `POST /auth/login`
  - `GET /me`
- Campuses:
  - `GET /campuses`
- Transfers:
  - `GET /transfers/availability?campusId=&shift=`
  - `POST /transfers` (incluye `modality`)
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
  - `GET /certificates/types`
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

## Branding y logo
- Escudo UMG utilizado desde fuente pública:  
  `https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Escudo_de_la_universidad_Mariano_G%C3%A1lvez_Guatemala.svg/250px-Escudo_de_la_universidad_Mariano_G%C3%A1lvez_Guatemala.svg.png`
- Archivo local: `frontend/academic-portal/public/assets/umg-shield.png`

## Arquitectura
Ver detalle en `docs/architecture.md`.
