using System.Globalization;
using System.Text;
using System.Text.Json;
using Academic.Application.Abstractions;
using Academic.Application.Common;
using Academic.Application.Contracts.Auth;
using Academic.Application.Contracts.Certificates;
using Academic.Application.Contracts.Common;
using Academic.Application.Contracts.Courses;
using Academic.Application.Contracts.Payments;
using Academic.Application.Contracts.Reports;
using Academic.Application.Contracts.Transfers;
using Academic.Domain.Enums;
using Academic.Domain.ValueObjects;
using Academic.Infrastructure.Configuration;
using Academic.Infrastructure.Persistence;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Academic.Infrastructure.Services;

public sealed class AcademicOperationsService(
    AcademicDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    IAuditLogService auditLogService,
    IEmailService emailService,
    IPdfService pdfService,
    IOptions<AuthOptions> authOptions,
    IOptions<JwtOptions> jwtOptions,
    IOptions<AcademicOptions> academicOptions,
    IHostEnvironment hostEnvironment,
    ILogger<AcademicOperationsService> logger)
    : IAuthService, ITransferService, ICourseService, IPaymentService, ICertificateService, IReportService
{
    private readonly AcademicDbContext _dbContext = dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IAuditLogService _auditLogService = auditLogService;
    private readonly IEmailService _emailService = emailService;
    private readonly IPdfService _pdfService = pdfService;
    private readonly AuthOptions _authOptions = authOptions.Value;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private readonly AcademicOptions _academicOptions = academicOptions.Value;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ILogger<AcademicOperationsService> _logger = logger;
    private static readonly string[] ApprovedHistoryStatuses =
    [
        "passed",
        "approved",
        "aprobado",
        "aprobada",
        "ganado",
        "ganada",
        "completed",
        "completado",
        "completada",
        "exonerado"
    ];
    private static readonly string[] FailedHistoryStatuses =
    [
        "failed",
        "reprobado",
        "reprobada",
        "desaprobado",
        "desaprobada",
        "perdido",
        "perdida",
        "no_aprobado",
        "no aprobado",
        "unapproved"
    ];
    private static readonly IReadOnlyList<CertificateTypeDefinition> CertificateTypes =
    [
        new("courses", "Certificacion de cursos", "Detalle de cursos aprobados por el estudiante.", false),
        new("enrollment", "Certificacion de matricula", "Constancia de inscripcion y matricula activa.", false),
        new("internships", "Certificacion de pasantias", "Constancia academica de pasantias registradas.", false),
        new("pensum-closure", "Cierre de pensum", "Disponible solo cuando el pensum completo esta aprobado.", true)
    ];

    private sealed record CertificateTypeDefinition(
        string Code,
        string Name,
        string Description,
        bool RequiresFullPensum);

    public async Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!InstitutionalEmail.TryCreate(normalizedEmail, _authOptions.AllowedEmailDomains, out _))
        {
            return Result<AuthResponseDto>.Failure("unauthorized", "Institutional email domain is not allowed.");
        }

        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("unauthorized", "Invalid credentials.");
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("unauthorized", "Invalid credentials.");
        }

        var role = user.UserRoles.Select(r => r.Role.Name).FirstOrDefault() ?? SystemRoles.Student;
        var token = _jwtTokenGenerator.GenerateToken(new JwtTokenContext(user.Id, user.Student?.Id, user.Email, role));
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        await _auditLogService.LogAsync(
            user.Id,
            "AuthLogin",
            "User",
            user.Id.ToString(),
            new { user.Email, role },
            null,
            cancellationToken);

        return Result<AuthResponseDto>.Success(
            new AuthResponseDto(token, expiresAt, role, user.Id, user.Student?.Id, user.Email));
    }

    public async Task<Result<MeDto>> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Include(x => x.Student)
                .ThenInclude(x => x!.Program)
            .Include(x => x.Student)
                .ThenInclude(x => x!.CurrentCampus)
            .Include(x => x.Student)
                .ThenInclude(x => x!.CurrentShift)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result<MeDto>.Failure("not_found", "User not found.");
        }

        var role = user.UserRoles.Select(r => r.Role.Name).FirstOrDefault() ?? SystemRoles.Student;
        string? fullName = null;
        string? carnet = null;
        string? programName = null;
        string? campusName = null;
        string? shiftName = null;
        short? currentCycle = null;

        if (user.Student is not null)
        {
            fullName = $"{user.Student.FirstName} {user.Student.LastName}".Trim();
            carnet = user.Student.Carnet;
            programName = user.Student.Program.Name;
            campusName = user.Student.CurrentCampus?.Name;
            shiftName = user.Student.CurrentShift?.Name;

            if (!Carnet.TryCreate(user.Student.Carnet, out var parsedCarnet))
            {
                return Result<MeDto>.Failure("validation_error", "El carnet del estudiante no tiene un formato válido.");
            }

            var prefix = await _dbContext.CarnetPrefixCatalogs
                .AsNoTracking()
                .Include(x => x.Campus)
                .Include(x => x.Shift)
                .Include(x => x.Program)
                .FirstOrDefaultAsync(x => x.Prefix == parsedCarnet!.Prefix && x.IsActive, cancellationToken);

            if (prefix is null)
            {
                return Result<MeDto>.Failure("validation_error", $"El prefijo de carnet {parsedCarnet!.Prefix} no está configurado.");
            }

            programName ??= prefix.Program.Name;
            campusName ??= prefix.Campus.Name;
            shiftName ??= prefix.Shift.Name;

            var maxPassedCycle = await BuildApprovedHistoryQuery(
                    _dbContext.StudentCourseHistories.AsNoTracking(),
                    user.Student.Id)
                .Join(_dbContext.Courses.AsNoTracking(),
                    history => history.CourseId,
                    course => course.Id,
                    (_, course) => (short?)course.Cycle)
                .MaxAsync(cancellationToken) ?? 0;

            currentCycle = maxPassedCycle <= 0
                ? (short)1
                : (short)Math.Min(10, maxPassedCycle + 1);
        }

        return Result<MeDto>.Success(new MeDto(
            user.Id,
            user.Student?.Id,
            user.Email,
            role,
            user.IsActive,
            fullName,
            carnet,
            programName,
            campusName,
            shiftName,
            currentCycle));
    }

    public async Task<Result<IReadOnlyList<CampusDto>>> GetCampusesAsync(CancellationToken cancellationToken)
    {
        var campuses = await _dbContext.Campuses
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CampusDto(x.Id, x.Code, x.Name, x.Address, x.IsActive, x.CampusType, x.Region))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CampusDto>>.Success(campuses);
    }

    public async Task<Result<TransferAvailabilityDto>> GetAvailabilityAsync(int campusId, string shift, CancellationToken cancellationToken)
    {
        var shiftName = NormalizeShift(shift);
        var availability = await _dbContext.CampusShiftCapacities
            .Include(x => x.Campus)
            .Include(x => x.Shift)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CampusId == campusId && x.Shift.Name == shiftName, cancellationToken);

        if (availability is null)
        {
            return Result<TransferAvailabilityDto>.Failure("not_found", "Campus/shift combination not found.");
        }

        return Result<TransferAvailabilityDto>.Success(new TransferAvailabilityDto(
            availability.CampusId,
            availability.Campus.Name,
            availability.ShiftId,
            availability.Shift.Name,
            availability.TotalCapacity,
            availability.OccupiedCapacity,
            availability.TotalCapacity - availability.OccupiedCapacity));
    }

    public async Task<Result<TransferCreateResultDto>> CreateTransferAsync(Guid studentId, CreateTransferDto request, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);
        var modality = NormalizeModality(request.Modality);
        if (modality is null)
        {
            return Result<TransferCreateResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["modality"] = ["La modalidad debe ser Presencial o Virtual."]
            });
        }

        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

        if (student is null || !student.IsActive)
        {
            return Result<TransferCreateResultDto>.Failure("not_found", "Student profile not found or inactive.");
        }

        if (student.CurrentCampusId == request.CampusId)
        {
            return Result<TransferCreateResultDto>.Failure("business_rule", "Transfer destination must be different from current campus.");
        }

        var hasActiveRequest = await _dbContext.TransferRequests
            .AnyAsync(x => x.StudentId == studentId &&
                (x.Status == DomainStatuses.Transfer.PendingPayment || x.Status == DomainStatuses.Transfer.PendingReview), cancellationToken);

        if (hasActiveRequest)
        {
            return Result<TransferCreateResultDto>.Failure("business_rule", "An active transfer request already exists.");
        }

        var shiftName = NormalizeShift(request.Shift);
        var availability = await _dbContext.CampusShiftCapacities
            .Include(x => x.Shift)
            .FirstOrDefaultAsync(x => x.CampusId == request.CampusId && x.Shift.Name == shiftName, cancellationToken);

        if (availability is null)
        {
            return Result<TransferCreateResultDto>.Failure("not_found", "Capacity for selected campus/shift not found.");
        }

        var availableSlots = availability.TotalCapacity - availability.OccupiedCapacity;
        if (availableSlots <= 0)
        {
            return Result<TransferCreateResultDto>.Failure("business_rule", "No available slots for selected campus/shift.");
        }

        var transferPricing = await GetServicePricingAsync("Transfer", student.ProgramId, cancellationToken);
        if (transferPricing is null || transferPricing.Amount <= 0)
        {
            return Result<TransferCreateResultDto>.Failure("config_error", "Transfer pricing is not configured.");
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(_academicOptions.PendingPaymentExpirationHours);

        var transfer = new TransferRequest
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            FromCampusId = student.CurrentCampusId,
            ToCampusId = request.CampusId,
            ToShiftId = availability.ShiftId,
            Modality = modality,
            Reason = request.Reason,
            Status = DomainStatuses.Transfer.PendingPayment,
            CreatedAt = now,
            UpdatedAt = now
        };

        var payment = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            OrderType = "Transfer",
            ReferenceId = transfer.Id,
            Amount = transferPricing.Amount,
            Currency = transferPricing.Currency,
            Status = DomainStatuses.Payment.Pending,
            Description = $"Pago de solicitud de traslado - Campus {request.CampusId} / {shiftName} / {modality}",
            CreatedAt = now,
            ExpiresAt = expiresAt
        };

        _dbContext.TransferRequests.Add(transfer);
        _dbContext.PaymentOrders.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            student.UserId,
            "TransferCreated",
            "TransferRequest",
            transfer.Id.ToString(),
            new { transfer.ToCampusId, transfer.ToShiftId, transfer.Modality, payment.Amount, payment.Currency, payment.ExpiresAt },
            null,
            cancellationToken);

        return Result<TransferCreateResultDto>.Success(
            new TransferCreateResultDto(transfer.Id, payment.Id, payment.Amount, payment.Currency, payment.ExpiresAt, transfer.Status));
    }

    public async Task<Result<IReadOnlyList<TransferDto>>> GetMyTransfersAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);

        var transfers = await _dbContext.TransferRequests
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .Include(x => x.Student)
            .Include(x => x.FromCampus)
            .Include(x => x.ToCampus)
            .Include(x => x.ToShift)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TransferDto(
                x.Id,
                x.Student.StudentCode,
                x.Student.FirstName + " " + x.Student.LastName,
                x.FromCampus != null ? x.FromCampus.Name : "N/A",
                x.ToCampus.Name,
                x.ToShift.Name,
                x.Modality,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TransferDto>>.Success(transfers);
    }

    public async Task<Result<TransferCancellationDto>> CancelTransferAsync(Guid studentId, Guid transferId, CancellationToken cancellationToken)
    {
        var transfer = await _dbContext.TransferRequests
            .FirstOrDefaultAsync(x => x.Id == transferId && x.StudentId == studentId, cancellationToken);

        if (transfer is null)
        {
            return Result<TransferCancellationDto>.Failure("not_found", "Transfer request not found.");
        }

        if (transfer.Status != DomainStatuses.Transfer.PendingPayment)
        {
            return Result<TransferCancellationDto>.Failure("business_rule", "Only pending-payment transfers can be cancelled.");
        }

        var payment = await _dbContext.PaymentOrders
            .FirstOrDefaultAsync(x => x.OrderType == "Transfer" && x.ReferenceId == transfer.Id, cancellationToken);

        if (payment is not null && payment.Status == DomainStatuses.Payment.Pending)
        {
            payment.Status = DomainStatuses.Payment.Cancelled;
            payment.CancelledAt = DateTime.UtcNow;
        }

        transfer.Status = DomainStatuses.Transfer.Cancelled;
        transfer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "TransferCancelled",
            "TransferRequest",
            transfer.Id.ToString(),
            new { transfer.StudentId },
            null,
            cancellationToken);

        return Result<TransferCancellationDto>.Success(new TransferCancellationDto(transfer.Id, transfer.Status, transfer.UpdatedAt));
    }

    public async Task<Result<TransferReviewResultDto>> ReviewTransferAsync(Guid adminUserId, Guid transferId, ReviewTransferDto request, CancellationToken cancellationToken)
    {
        var transfer = await _dbContext.TransferRequests
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == transferId, cancellationToken);

        if (transfer is null)
        {
            return Result<TransferReviewResultDto>.Failure("not_found", "Transfer request not found.");
        }

        if (transfer.Status != DomainStatuses.Transfer.PendingReview)
        {
            return Result<TransferReviewResultDto>.Failure("business_rule", "Only transfers pending review can be reviewed.");
        }

        var payment = await _dbContext.PaymentOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderType == "Transfer" && x.ReferenceId == transfer.Id, cancellationToken);

        if (payment is null || payment.Status != DomainStatuses.Payment.Paid)
        {
            return Result<TransferReviewResultDto>.Failure("business_rule", "Transfer must have a paid order before review.");
        }

        var normalizedDecision = request.Decision.Trim().ToLowerInvariant();
        if (normalizedDecision is not ("approved" or "rejected"))
        {
            return Result<TransferReviewResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["decision"] = ["Decision must be Approved or Rejected."]
            });
        }

        if (normalizedDecision == "approved")
        {
            var destinationCapacity = await _dbContext.CampusShiftCapacities
                .FirstOrDefaultAsync(x => x.CampusId == transfer.ToCampusId && x.ShiftId == transfer.ToShiftId, cancellationToken);

            if (destinationCapacity is null)
            {
                return Result<TransferReviewResultDto>.Failure("not_found", "Destination capacity record not found.");
            }

            if (destinationCapacity.OccupiedCapacity >= destinationCapacity.TotalCapacity)
            {
                return Result<TransferReviewResultDto>.Failure("business_rule", "Destination campus/shift has no available capacity.");
            }

            destinationCapacity.OccupiedCapacity += 1;
            destinationCapacity.UpdatedAt = DateTime.UtcNow;

            if (transfer.Student.CurrentCampusId.HasValue && transfer.Student.CurrentShiftId.HasValue)
            {
                var originCapacity = await _dbContext.CampusShiftCapacities
                    .FirstOrDefaultAsync(x =>
                        x.CampusId == transfer.Student.CurrentCampusId.Value &&
                        x.ShiftId == transfer.Student.CurrentShiftId.Value,
                        cancellationToken);

                if (originCapacity is not null && originCapacity.OccupiedCapacity > 0)
                {
                    originCapacity.OccupiedCapacity -= 1;
                    originCapacity.UpdatedAt = DateTime.UtcNow;
                }
            }

            transfer.Student.CurrentCampusId = transfer.ToCampusId;
            transfer.Student.CurrentShiftId = transfer.ToShiftId;
            transfer.Student.UpdatedAt = DateTime.UtcNow;
            transfer.Status = DomainStatuses.Transfer.Approved;
        }
        else
        {
            transfer.Status = DomainStatuses.Transfer.Rejected;
        }

        transfer.ReviewedByUserId = adminUserId;
        transfer.ReviewedAt = DateTime.UtcNow;
        transfer.ReviewNotes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        transfer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            adminUserId,
            "TransferReviewed",
            "TransferRequest",
            transfer.Id.ToString(),
            new { transfer.Status, transfer.ReviewNotes },
            null,
            cancellationToken);

        return Result<TransferReviewResultDto>.Success(new TransferReviewResultDto(
            transfer.Id,
            transfer.Status,
            transfer.ReviewedByUserId,
            transfer.ReviewedAt,
            transfer.ReviewNotes));
    }

    public async Task<Result<IReadOnlyList<CourseDto>>> GetPensumAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

        if (student is null)
        {
            return Result<IReadOnlyList<CourseDto>>.Failure("not_found", "Student profile not found.");
        }

        var overdueIds = await GetOverdueIdsAsync(studentId, cancellationToken);
        var approvedCourseIds = await BuildApprovedHistoryQuery(
                _dbContext.StudentCourseHistories.AsNoTracking(),
                studentId)
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var approvedCourseIdSet = approvedCourseIds.ToHashSet();

        var coursesRaw = await _dbContext.Courses
            .AsNoTracking()
            .Include(c => c.PrerequisiteCourses)
            .Include(c => c.CourseCreditRequirements)
            .Where(x => x.ProgramId == student.ProgramId && x.IsActive)
            .OrderBy(x => x.Cycle)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var courses = coursesRaw.Select(x =>
        {
            var prerequisiteCodes = x.PrerequisiteCourses
                .Select(c => c.Code)
                .Distinct()
                .OrderBy(code => code)
                .ToList();
            var prerequisiteCredits = x.CourseCreditRequirements
                .Select(c => c.MinApprovedCredits)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            var prerequisiteSummary = BuildPrerequisiteSummary(prerequisiteCodes, prerequisiteCredits);
            return new CourseDto(
                x.Id,
                x.Code,
                x.Name,
                x.Credits,
                x.Cycle,
                x.HoursPerWeek,
                x.HoursTotal,
                x.IsLab,
                approvedCourseIdSet.Contains(x.Id),
                overdueIds.Contains(x.Id),
                prerequisiteCodes.Count > 0 || prerequisiteCredits.Count > 0,
                prerequisiteSummary);
        }).ToList();

        return Result<IReadOnlyList<CourseDto>>.Success(courses);
    }

    public async Task<Result<IReadOnlyList<OverdueCourseDto>>> GetOverdueAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var overdueIds = await GetOverdueIdsAsync(studentId, cancellationToken);

        var overdueCourses = await _dbContext.Courses
            .AsNoTracking()
            .Where(x => overdueIds.Contains(x.Id))
            .OrderBy(x => x.Code)
            .Select(x => new OverdueCourseDto(x.Id, x.Code, x.Name, x.Credits))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OverdueCourseDto>>.Success(overdueCourses);
    }

    public async Task<Result<IReadOnlyList<EnrollmentSummaryDto>>> GetMyEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);

        var enrollments = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new EnrollmentSummaryDto(
                x.Id,
                x.EnrollmentType,
                x.Status,
                x.TotalAmount,
                _dbContext.PaymentOrders
                    .Where(po => po.OrderType == "Enrollment" && po.ReferenceId == x.Id)
                    .OrderByDescending(po => po.CreatedAt)
                    .Select(po => po.Currency)
                    .FirstOrDefault() ?? _academicOptions.DefaultCurrency,
                _dbContext.PaymentOrders
                    .Where(po => po.OrderType == "Enrollment" && po.ReferenceId == x.Id)
                    .OrderByDescending(po => po.CreatedAt)
                    .Select(po => po.Id)
                    .FirstOrDefault(),
                _dbContext.PaymentOrders
                    .Where(po => po.OrderType == "Enrollment" && po.ReferenceId == x.Id)
                    .OrderByDescending(po => po.CreatedAt)
                    .Select(po => po.ExpiresAt)
                    .FirstOrDefault(),
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EnrollmentSummaryDto>>.Success(enrollments);
    }

    public async Task<Result<EnrollmentResultDto>> CreateEnrollmentAsync(Guid studentId, CreateEnrollmentDto request, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);

        var requestedSelections = request.CourseSelections ?? Array.Empty<EnrollmentCourseSelectionDto>();
        var validSelections = requestedSelections
            .Where(x => x.CourseId > 0 && !string.IsNullOrWhiteSpace(x.Shift))
            .ToList();

        if (validSelections.Count == 0)
        {
            return Result<EnrollmentResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["courseSelections"] = ["Debe seleccionar al menos un curso válido."]
            });
        }

        if (validSelections.Count > 6)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "No puedes asignarte más de 6 cursos por solicitud.");
        }

        var duplicateCourseIds = validSelections
            .GroupBy(x => x.CourseId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicateCourseIds.Count > 0)
        {
            return Result<EnrollmentResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["courseSelections"] = ["No puedes repetir cursos en la misma solicitud."]
            });
        }

        var uniqueIds = validSelections.Select(x => x.CourseId).ToList();

        var student = await _dbContext.Students
            .AsNoTracking()
            .Include(x => x.CarnetPrefixNavigation)
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

        if (student is null)
        {
            return Result<EnrollmentResultDto>.Failure("not_found", "No se encontró el perfil del estudiante.");
        }

        var shifts = await _dbContext.Shifts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var shiftByName = shifts.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        if (!shiftByName.TryGetValue("Saturday", out var saturdayShift) ||
            !shiftByName.TryGetValue("Sunday", out var sundayShift))
        {
            return Result<EnrollmentResultDto>.Failure("config_error", "No existe configuración de jornadas (Saturday/Sunday).");
        }

        var shiftIdByCourse = new Dictionary<int, short>();
        foreach (var selection in validSelections)
        {
            var normalizedShift = NormalizeShift(selection.Shift);
            if (!shiftByName.TryGetValue(normalizedShift, out var shift))
            {
                return Result<EnrollmentResultDto>.ValidationFailure(new Dictionary<string, string[]>
                {
                    ["courseSelections"] = [$"La jornada '{selection.Shift}' no es válida. Usa Saturday o Sunday."]
                });
            }

            shiftIdByCourse[selection.CourseId] = shift.Id;
        }

        var primaryShiftId = student.CurrentShiftId;
        primaryShiftId ??= student.CarnetPrefixNavigation?.ShiftId;
        primaryShiftId ??= saturdayShift.Id;

        var saturdayCount = shiftIdByCourse.Values.Count(x => x == saturdayShift.Id);
        var sundayCount = shiftIdByCourse.Values.Count(x => x == sundayShift.Id);

        if (primaryShiftId == saturdayShift.Id && saturdayCount <= sundayCount)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "Distribución inválida: para plan sábado debes llevar mayoría de cursos en sábado.");
        }

        if (primaryShiftId == sundayShift.Id && sundayCount <= saturdayCount)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "Distribución inválida: para plan domingo debes llevar mayoría de cursos en domingo.");
        }

        var hasPendingEnrollment = await _dbContext.Enrollments
            .AnyAsync(x => x.StudentId == studentId && x.Status == DomainStatuses.Enrollment.PendingPayment, cancellationToken);

        if (hasPendingEnrollment)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "Ya tienes una solicitud de asignación activa.");
        }

        var selectedCourses = await _dbContext.Courses
            .Include(c => c.PrerequisiteCourses)
            .Include(c => c.CourseCreditRequirements)
            .Where(x => uniqueIds.Contains(x.Id) && x.ProgramId == student.ProgramId && x.IsActive)
            .ToListAsync(cancellationToken);

        if (selectedCourses.Count != uniqueIds.Count)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "Uno o más cursos seleccionados no son válidos para tu carrera.");
        }

        var confirmedEnrollmentCounts = await _dbContext.EnrollmentCourses
            .Join(_dbContext.Enrollments,
                ec => ec.EnrollmentId,
                e => e.Id,
                (ec, e) => new { ec.CourseId, e.Status })
            .Where(x => uniqueIds.Contains(x.CourseId) && x.Status == DomainStatuses.Enrollment.Confirmed)
            .GroupBy(x => x.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countLookup = confirmedEnrollmentCounts.ToDictionary(x => x.CourseId, x => x.Count);
        foreach (var course in selectedCourses)
        {
            countLookup.TryGetValue(course.Id, out var occupied);
            if (occupied >= _academicOptions.DefaultCourseCapacity)
            {
                return Result<EnrollmentResultDto>.Failure("business_rule", $"No hay cupo disponible para el curso {course.Code}.");
            }
        }

        var passedCourses = await BuildApprovedHistoryQuery(
                _dbContext.StudentCourseHistories,
                studentId)
            .Join(_dbContext.Courses.AsNoTracking(),
                history => history.CourseId,
                course => course.Id,
                (history, course) => new { history.CourseId, course.Credits, course.Cycle })
            .Distinct()
            .ToListAsync(cancellationToken);

        var passedCourseIdSet = passedCourses.Select(x => x.CourseId).ToHashSet();
        var approvedCredits = passedCourses.Sum(x => (int)x.Credits);
        var currentAcademicCycle = passedCourses.Count == 0
            ? (short)1
            : (short)Math.Min(10, passedCourses.Max(x => x.Cycle) + 1);
        var isEarlyCycleStudent = currentAcademicCycle <= 3;

        foreach (var course in selectedCourses)
        {
            var prerequisiteIds = course.PrerequisiteCourses.Select(x => x.Id).Distinct().ToList();
            var minCredits = course.CourseCreditRequirements
                .Select(x => x.MinApprovedCredits)
                .DefaultIfEmpty((short)0)
                .Max();

            var canAdvanceWithoutPrerequisites = isEarlyCycleStudent && prerequisiteIds.Count == 0;
            if (!canAdvanceWithoutPrerequisites)
            {
                var unmet = prerequisiteIds.Where(id => !passedCourseIdSet.Contains(id)).ToList();
                if (unmet.Count > 0)
                {
                    var missingCodes = course.PrerequisiteCourses
                        .Where(x => unmet.Contains(x.Id))
                        .Select(x => x.Code)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
                    var missingDetail = missingCodes.Count > 0
                        ? $" Faltan: {string.Join(", ", missingCodes)}."
                        : string.Empty;
                    return Result<EnrollmentResultDto>.Failure("business_rule", $"No cumples los prerrequisitos para el curso {course.Code}.{missingDetail}");
                }

                if (approvedCredits < minCredits)
                {
                    return Result<EnrollmentResultDto>.Failure(
                        "business_rule",
                        $"No cumples el mínimo de créditos aprobados para {course.Code}. Requiere {minCredits}.");
                }
            }
        }

        var overdueIds = await GetOverdueIdsAsync(studentId, cancellationToken);
        var approvedSelections = uniqueIds.Where(id => passedCourseIdSet.Contains(id)).ToList();
        if (approvedSelections.Count > 0)
        {
            var approvedCodes = selectedCourses
                .Where(x => approvedSelections.Contains(x.Id))
                .Select(x => x.Code)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            var approvedDetail = approvedCodes.Count > 0
                ? $" Cursos ya aprobados: {string.Join(", ", approvedCodes)}."
                : string.Empty;
            return Result<EnrollmentResultDto>.Failure("business_rule", $"No puedes asignarte cursos que ya tienes aprobados.{approvedDetail}");
        }

        var courseExtraPricing = await GetServicePricingAsync("CourseExtra", student.ProgramId, cancellationToken);
        var courseOverduePricing = await GetServicePricingAsync("CourseOverdue", student.ProgramId, cancellationToken);

        if (courseExtraPricing is null || courseOverduePricing is null ||
            courseExtraPricing.Amount <= 0 || courseOverduePricing.Amount <= 0)
        {
            return Result<EnrollmentResultDto>.Failure("config_error", "Enrollment pricing is not configured.");
        }

        if (!string.Equals(courseExtraPricing.Currency, courseOverduePricing.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return Result<EnrollmentResultDto>.Failure("config_error", "Pricing currency mismatch between extra and overdue courses.");
        }

        var totalAmount = 0m;
        var hasOverdue = false;
        var hasExtra = false;

        foreach (var courseId in uniqueIds)
        {
            var isOverdue = overdueIds.Contains(courseId);
            totalAmount += isOverdue ? courseOverduePricing.Amount : courseExtraPricing.Amount;
            hasOverdue |= isOverdue;
            hasExtra |= !isOverdue;
        }

        var enrollmentType = hasOverdue && hasExtra
            ? "Mixed"
            : hasOverdue ? "Overdue" : "Extra";

        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(_academicOptions.PendingPaymentExpirationHours);

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            EnrollmentType = enrollmentType,
            Status = DomainStatuses.Enrollment.PendingPayment,
            TotalAmount = totalAmount,
            CreatedAt = now,
            UpdatedAt = now
        };

        var payment = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            OrderType = "Enrollment",
            ReferenceId = enrollment.Id,
            Amount = totalAmount,
            Currency = courseExtraPricing.Currency,
            Status = DomainStatuses.Payment.Pending,
            Description = "Pago de asignacion de cursos",
            CreatedAt = now,
            ExpiresAt = expiresAt
        };

        _dbContext.Enrollments.Add(enrollment);
        _dbContext.PaymentOrders.Add(payment);

        foreach (var courseId in uniqueIds)
        {
            _dbContext.EnrollmentCourses.Add(new EnrollmentCourse
            {
                EnrollmentId = enrollment.Id,
                CourseId = courseId,
                ShiftId = shiftIdByCourse[courseId],
                IsOverdue = overdueIds.Contains(courseId)
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            student.UserId,
            "EnrollmentCreated",
            "Enrollment",
            enrollment.Id.ToString(),
            new
            {
                enrollment.EnrollmentType,
                enrollment.TotalAmount,
                payment.Currency,
                payment.ExpiresAt,
                courseSelections = validSelections.Select(x => new { x.CourseId, Shift = NormalizeShift(x.Shift) }).ToList()
            },
            null,
            cancellationToken);

        return Result<EnrollmentResultDto>.Success(
            new EnrollmentResultDto(enrollment.Id, payment.Id, totalAmount, payment.Currency, payment.ExpiresAt, enrollment.Status));
    }

    public async Task<Result<EnrollmentCancellationDto>> CancelEnrollmentAsync(Guid studentId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _dbContext.Enrollments
            .FirstOrDefaultAsync(x => x.Id == enrollmentId && x.StudentId == studentId, cancellationToken);

        if (enrollment is null)
        {
            return Result<EnrollmentCancellationDto>.Failure("not_found", "Enrollment request not found.");
        }

        if (enrollment.Status != DomainStatuses.Enrollment.PendingPayment)
        {
            return Result<EnrollmentCancellationDto>.Failure("business_rule", "Only pending-payment enrollments can be cancelled.");
        }

        var payment = await _dbContext.PaymentOrders
            .FirstOrDefaultAsync(x => x.OrderType == "Enrollment" && x.ReferenceId == enrollment.Id, cancellationToken);

        if (payment is not null && payment.Status == DomainStatuses.Payment.Pending)
        {
            payment.Status = DomainStatuses.Payment.Cancelled;
            payment.CancelledAt = DateTime.UtcNow;
        }

        enrollment.Status = DomainStatuses.Enrollment.Cancelled;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "EnrollmentCancelled",
            "Enrollment",
            enrollment.Id.ToString(),
            new { enrollment.StudentId },
            null,
            cancellationToken);

        return Result<EnrollmentCancellationDto>.Success(new EnrollmentCancellationDto(enrollment.Id, enrollment.Status, enrollment.UpdatedAt));
    }

    public async Task<Result<FilePayloadDto>> DownloadEnrollmentDireAsync(
        Guid studentId,
        Guid enrollmentId,
        CancellationToken cancellationToken)
    {
        var direResult = await EnsureEnrollmentDireGeneratedAsync(enrollmentId, studentId, cancellationToken);
        if (!direResult.IsSuccess || direResult.Value is null)
        {
            return Result<FilePayloadDto>.Failure(
                direResult.Error?.Code ?? "not_found",
                direResult.Error?.Message ?? "No fue posible generar el DIRE de inscripción.");
        }

        var artifact = direResult.Value;
        var bytes = await File.ReadAllBytesAsync(artifact.FullPath, cancellationToken);
        return Result<FilePayloadDto>.Success(new FilePayloadDto(bytes, "application/pdf", artifact.FileName));
    }

    public async Task<Result<IReadOnlyList<PaymentOrderDto>>> GetMyPaymentsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);

        var payments = await _dbContext.PaymentOrders
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentOrderDto(
                x.Id,
                x.OrderType,
                x.ReferenceId,
                x.Amount,
                x.Currency,
                x.Status,
                x.Description,
                x.CreatedAt,
                x.ExpiresAt,
                x.PaidAt,
                x.CancelledAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PaymentOrderDto>>.Success(payments);
    }

    public async Task<Result<IReadOnlyList<PaymentOrderDto>>> GetPendingPaymentsAsync(CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(null, cancellationToken);

        var payments = await _dbContext.PaymentOrders
            .AsNoTracking()
            .Where(x => x.Status == DomainStatuses.Payment.Pending)
            .OrderBy(x => x.ExpiresAt)
            .Select(x => new PaymentOrderDto(
                x.Id,
                x.OrderType,
                x.ReferenceId,
                x.Amount,
                x.Currency,
                x.Status,
                x.Description,
                x.CreatedAt,
                x.ExpiresAt,
                x.PaidAt,
                x.CancelledAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PaymentOrderDto>>.Success(payments);
    }

    public async Task<Result<PaymentOrderDto>> MarkPaidAsync(Guid paymentId, Guid actedByUserId, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.PaymentOrders
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return Result<PaymentOrderDto>.Failure("not_found", "Payment order not found.");
        }

        return await MarkPaymentAsPaidInternalAsync(payment, actedByUserId, cancellationToken);
    }

    public async Task<Result<MockCheckoutResultDto>> MockCheckoutAsync(
        Guid paymentId,
        Guid studentId,
        Guid actedByUserId,
        MockCheckoutRequestDto request,
        CancellationToken cancellationToken)
    {
        var cardValidation = ValidateMockCard(request);
        if (cardValidation.Count > 0)
        {
            return Result<MockCheckoutResultDto>.ValidationFailure(cardValidation);
        }

        var payment = await _dbContext.PaymentOrders
            .FirstOrDefaultAsync(x => x.Id == paymentId && x.StudentId == studentId, cancellationToken);

        if (payment is null)
        {
            return Result<MockCheckoutResultDto>.Failure("not_found", "No se encontró la orden de pago.");
        }

        if (payment.Status != DomainStatuses.Payment.Pending)
        {
            return Result<MockCheckoutResultDto>.Failure("business_rule", "La orden de pago ya no está pendiente.");
        }

        var paidResult = await MarkPaymentAsPaidInternalAsync(payment, actedByUserId, cancellationToken);
        if (!paidResult.IsSuccess || paidResult.Value is null)
        {
            return Result<MockCheckoutResultDto>.Failure(
                paidResult.Error?.Code ?? "payment_error",
                paidResult.Error?.Message ?? "No fue posible aprobar el pago.");
        }

        MockCheckoutCertificateDto? certificate = null;
        MockCheckoutEnrollmentDireDto? enrollmentDire = null;
        if (string.Equals(payment.OrderType, "Certificate", StringComparison.OrdinalIgnoreCase))
        {
            var certificateGeneration = await TryGenerateCertificateFromPaymentAsync(studentId, payment.Id, cancellationToken);
            if (!certificateGeneration.IsSuccess)
            {
                return Result<MockCheckoutResultDto>.Failure(
                    certificateGeneration.Error?.Code ?? "certificate_error",
                    certificateGeneration.Error?.Message ?? "Pago aprobado, pero no se pudo generar el certificado.");
            }

            certificate = certificateGeneration.Value;
        }
        else if (string.Equals(payment.OrderType, "Enrollment", StringComparison.OrdinalIgnoreCase))
        {
            var direGeneration = await EnsureEnrollmentDireGeneratedAsync(payment.ReferenceId, studentId, cancellationToken);
            if (direGeneration.IsSuccess && direGeneration.Value is not null)
            {
                enrollmentDire = new MockCheckoutEnrollmentDireDto(
                    payment.ReferenceId,
                    direGeneration.Value.DireNumber,
                    File.Exists(direGeneration.Value.FullPath));
            }
        }

        var maskedCard = MaskCardNumber(request.CardNumber);
        await _auditLogService.LogAsync(
            actedByUserId,
            "PaymentMockCheckoutApproved",
            "PaymentOrder",
            payment.Id.ToString(),
            new
            {
                payment.OrderType,
                payment.ReferenceId,
                cardHolder = request.CardHolderName.Trim(),
                cardMasked = maskedCard
            },
            null,
            cancellationToken);

        return Result<MockCheckoutResultDto>.Success(new MockCheckoutResultDto(paidResult.Value, certificate, enrollmentDire));
    }

    public Task<Result<IReadOnlyList<CertificateTypeDto>>> GetTypesAsync(CancellationToken cancellationToken)
    {
        var types = CertificateTypes
            .Select(x => new CertificateTypeDto(x.Code, x.Name, x.Description, x.RequiresFullPensum))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CertificateTypeDto>>.Success(types));
    }

    public async Task<Result<CertificateCreatedDto>> CreateAsync(Guid studentId, CreateCertificateDto request, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);

        var student = await _dbContext.Students
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

        if (student is null)
        {
            return Result<CertificateCreatedDto>.Failure("not_found", "Student profile not found.");
        }

        var activeCertificate = await _dbContext.Certificates.AnyAsync(
            x => x.StudentId == studentId &&
                (x.Status == DomainStatuses.Certificate.Requested || x.Status == DomainStatuses.Certificate.PdfGenerated),
            cancellationToken);

        if (activeCertificate)
        {
            return Result<CertificateCreatedDto>.Failure("business_rule", "Ya tienes una solicitud de certificacion activa.");
        }

        var certificateType = ResolveCertificateType(request.Purpose);
        if (certificateType is null)
        {
            return Result<CertificateCreatedDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["purpose"] = [$"Tipo de certificacion no valido. Opciones: {string.Join(", ", CertificateTypes.Select(x => x.Name))}."]
            });
        }

        if (certificateType.RequiresFullPensum)
        {
            var requiredCourses = await _dbContext.Courses
                .AsNoTracking()
                .Where(x => x.ProgramId == student.ProgramId && x.IsActive)
                .Select(x => new { x.Id, x.Code, x.Name })
                .ToListAsync(cancellationToken);

            if (requiredCourses.Count == 0)
            {
                return Result<CertificateCreatedDto>.Failure("config_error", "No hay cursos activos configurados para este programa.");
            }

            var approvedIds = await BuildApprovedHistoryQuery(
                    _dbContext.StudentCourseHistories.AsNoTracking(),
                    studentId)
                .Select(x => x.CourseId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var approvedIdSet = approvedIds.ToHashSet();
            var pendingCourses = requiredCourses
                .Where(x => !approvedIdSet.Contains(x.Id))
                .OrderBy(x => x.Code)
                .ToList();

            if (pendingCourses.Count > 0)
            {
                var pendingPreview = string.Join(", ", pendingCourses.Take(4).Select(x => $"{x.Code} {x.Name}"));
                return Result<CertificateCreatedDto>.Failure(
                    "business_rule",
                    $"Para solicitar Cierre de pensum debes tener todos los cursos aprobados. Pendientes: {pendingCourses.Count}. {pendingPreview}");
            }
        }

        var certificatePricing = await GetServicePricingAsync("Certificate", student.ProgramId, cancellationToken);
        if (certificatePricing is null || certificatePricing.Amount <= 0)
        {
            return Result<CertificateCreatedDto>.Failure("config_error", "No existe configuracion de precio para certificaciones.");
        }

        var now = DateTime.UtcNow;
        var certificateId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var verificationCode = GenerateVerificationCode();

        var payment = new PaymentOrder
        {
            Id = paymentId,
            StudentId = studentId,
            OrderType = "Certificate",
            ReferenceId = certificateId,
            Amount = certificatePricing.Amount,
            Currency = certificatePricing.Currency,
            Status = DomainStatuses.Payment.Pending,
            Description = $"Pago {certificateType.Name}",
            CreatedAt = now,
            ExpiresAt = now.AddHours(_academicOptions.PendingPaymentExpirationHours)
        };

        var certificate = new Certificate
        {
            Id = certificateId,
            StudentId = studentId,
            PaymentOrderId = paymentId,
            Purpose = certificateType.Name,
            Status = DomainStatuses.Certificate.Requested,
            VerificationCode = verificationCode,
            CreatedAt = now,
            Metadata = JsonSerializer.Serialize(new { source = "api", requestedBy = student.User.Email, certificateType = certificateType.Code })
        };

        _dbContext.PaymentOrders.Add(payment);
        _dbContext.Certificates.Add(certificate);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            student.UserId,
            "CertificateRequested",
            "Certificate",
            certificate.Id.ToString(),
            new { certificate.Purpose, payment.Amount, payment.Currency, payment.ExpiresAt },
            null,
            cancellationToken);

        return Result<CertificateCreatedDto>.Success(new CertificateCreatedDto(
            certificate.Id,
            payment.Id,
            payment.Amount,
            payment.Currency,
            payment.ExpiresAt,
            certificate.Status,
            certificate.VerificationCode));
    }

    public async Task<Result<CertificateDto>> GenerateAsync(Guid actorStudentId, Guid certificateId, GenerateCertificateDto request, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(actorStudentId, cancellationToken);

        var certificate = await _dbContext.Certificates
            .Include(x => x.PaymentOrder)
            .Include(x => x.Student)
                .ThenInclude(x => x.Program)
            .Include(x => x.Student)
                .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == certificateId, cancellationToken);

        if (certificate is null)
        {
            return Result<CertificateDto>.Failure("not_found", "Certificate not found.");
        }

        if (certificate.StudentId != actorStudentId)
        {
            return Result<CertificateDto>.Failure("forbidden", "Certificate does not belong to current student.");
        }

        if (certificate.PaymentOrder.Status != DomainStatuses.Payment.Paid)
        {
            return Result<CertificateDto>.Failure("business_rule", "Certificate can only be generated after payment is confirmed.");
        }

        var approvedCourses = await BuildApprovedHistoryQuery(
                _dbContext.StudentCourseHistories.AsNoTracking(),
                actorStudentId)
            .Include(x => x.Course)
            .OrderBy(x => x.Course.Code)
            .Select(x => x.Course.Code + " - " + x.Course.Name)
            .ToListAsync(cancellationToken);

        var model = new CertificatePdfModel(
            StudentName: certificate.Student.FirstName + " " + certificate.Student.LastName,
            StudentCode: certificate.Student.StudentCode,
            ProgramName: certificate.Student.Program.Name,
            Purpose: certificate.Purpose,
            VerificationCode: certificate.VerificationCode,
            GeneratedAt: DateTime.UtcNow,
            ApprovedCourses: approvedCourses,
            IncludeQr: request.IncludeQr
        );

        var pdfBytes = _pdfService.BuildCertificatePdf(model);
        var relativeDir = _academicOptions.CertificatesStoragePath;
        var fullDir = Path.Combine(_hostEnvironment.ContentRootPath, relativeDir);
        Directory.CreateDirectory(fullDir);

        var fileName = $"certificate-{certificate.Id}.pdf";
        var absolutePath = Path.Combine(fullDir, fileName);
        await File.WriteAllBytesAsync(absolutePath, pdfBytes, cancellationToken);

        certificate.PdfPath = absolutePath;
        certificate.Status = request.SendEmail ? DomainStatuses.Certificate.Sent : DomainStatuses.Certificate.PdfGenerated;
        certificate.GeneratedAt = DateTime.UtcNow;
        if (request.SendEmail)
        {
            certificate.SentAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (request.SendEmail)
        {
            await _emailService.SendAsync(
                certificate.Student.InstitutionalEmail,
                "Certificacion digital generada",
                $"<p>Tu certificacion digital fue generada.</p><p>Codigo: <strong>{certificate.VerificationCode}</strong></p>",
                cancellationToken);
        }

        await _auditLogService.LogAsync(
            certificate.Student.UserId,
            "CertificateGenerated",
            "Certificate",
            certificate.Id.ToString(),
            new { certificate.Status, certificate.PdfPath },
            null,
            cancellationToken);

        return Result<CertificateDto>.Success(new CertificateDto(
            certificate.Id,
            certificate.Status,
            certificate.VerificationCode,
            certificate.PdfPath,
            certificate.CreatedAt,
            certificate.GeneratedAt,
            certificate.SentAt));
    }

    public async Task<Result<IReadOnlyList<CertificateSummaryDto>>> GetMyCertificatesAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(studentId, cancellationToken);

        var rows = await _dbContext.Certificates
            .AsNoTracking()
            .Include(x => x.PaymentOrder)
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CertificateSummaryDto(
                x.Id,
                x.Purpose,
                x.Status,
                x.PaymentOrder.Status,
                !string.IsNullOrWhiteSpace(x.PdfPath),
                x.VerificationCode,
                x.PaymentOrderId,
                x.PaymentOrder.Amount,
                x.PaymentOrder.Currency,
                x.PaymentOrder.ExpiresAt,
                x.CreatedAt,
                x.GeneratedAt,
                x.SentAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CertificateSummaryDto>>.Success(rows);
    }

    public async Task<Result<CertificateCancellationDto>> CancelAsync(Guid studentId, Guid certificateId, CancellationToken cancellationToken)
    {
        var certificate = await _dbContext.Certificates
            .Include(x => x.PaymentOrder)
            .FirstOrDefaultAsync(x => x.Id == certificateId && x.StudentId == studentId, cancellationToken);

        if (certificate is null)
        {
            return Result<CertificateCancellationDto>.Failure("not_found", "Certificate request not found.");
        }

        if (certificate.Status != DomainStatuses.Certificate.Requested)
        {
            return Result<CertificateCancellationDto>.Failure("business_rule", "Only requested certificates can be cancelled.");
        }

        if (certificate.PaymentOrder.Status == DomainStatuses.Payment.Pending)
        {
            certificate.PaymentOrder.Status = DomainStatuses.Payment.Cancelled;
            certificate.PaymentOrder.CancelledAt = DateTime.UtcNow;
        }

        certificate.Status = DomainStatuses.Certificate.Cancelled;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "CertificateCancelled",
            "Certificate",
            certificate.Id.ToString(),
            new { certificate.StudentId },
            null,
            cancellationToken);

        return Result<CertificateCancellationDto>.Success(new CertificateCancellationDto(certificate.Id, certificate.Status));
    }

    public async Task<Result<FilePayloadDto>> DownloadAsync(Guid actorStudentId, Guid certificateId, CancellationToken cancellationToken)
    {
        var certificate = await _dbContext.Certificates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == certificateId && x.StudentId == actorStudentId, cancellationToken);

        if (certificate is null)
        {
            return Result<FilePayloadDto>.Failure("not_found", "Certificate not found.");
        }

        if (string.IsNullOrWhiteSpace(certificate.PdfPath) || !File.Exists(certificate.PdfPath))
        {
            return Result<FilePayloadDto>.Failure("not_found", "PDF file was not generated yet.");
        }

        var bytes = await File.ReadAllBytesAsync(certificate.PdfPath, cancellationToken);
        return Result<FilePayloadDto>.Success(new FilePayloadDto(
            bytes,
            "application/pdf",
            Path.GetFileName(certificate.PdfPath)));
    }

    public async Task<Result<CertificateVerificationDto>> VerifyAsync(string verificationCode, CancellationToken cancellationToken)
    {
        var normalized = verificationCode.Trim();
        var certificate = await _dbContext.Certificates
            .AsNoTracking()
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.VerificationCode == normalized, cancellationToken);

        if (certificate is null)
        {
            return Result<CertificateVerificationDto>.Success(new CertificateVerificationDto(false, "Codigo no encontrado.", null, null));
        }

        var valid = certificate.Status is DomainStatuses.Certificate.PdfGenerated or DomainStatuses.Certificate.Sent;
        var studentName = certificate.Student.FirstName + " " + certificate.Student.LastName;

        return Result<CertificateVerificationDto>.Success(new CertificateVerificationDto(
            valid,
            valid ? "Certificacion valida." : "Certificacion no valida para uso.",
            studentName,
            certificate.GeneratedAt));
    }

    public async Task<Result<IReadOnlyList<TransferReportRowDto>>> GetTransfersReportAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.TransferRequests
            .AsNoTracking()
            .Include(x => x.Student)
            .Include(x => x.FromCampus)
            .Include(x => x.ToCampus)
            .Include(x => x.ToShift)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TransferReportRowDto(
                x.Id,
                x.Student.StudentCode,
                x.Student.FirstName + " " + x.Student.LastName,
                x.FromCampus != null ? x.FromCampus.Name : "N/A",
                x.ToCampus.Name,
                x.ToShift.Name,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TransferReportRowDto>>.Success(rows);
    }

    public async Task<Result<IReadOnlyList<EnrollmentReportRowDto>>> GetEnrollmentsReportAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Enrollments
            .AsNoTracking()
            .Include(x => x.Student)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new EnrollmentReportRowDto(
                x.Id,
                x.Student.StudentCode,
                x.Student.FirstName + " " + x.Student.LastName,
                x.EnrollmentType,
                x.Status,
                x.TotalAmount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<EnrollmentReportRowDto>>.Success(rows);
    }

    public async Task<Result<IReadOnlyList<CertificateReportRowDto>>> GetCertificatesReportAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.Certificates
            .AsNoTracking()
            .Include(x => x.Student)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CertificateReportRowDto(
                x.Id,
                x.Student.StudentCode,
                x.Student.FirstName + " " + x.Student.LastName,
                x.Status,
                x.VerificationCode,
                x.CreatedAt,
                x.GeneratedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CertificateReportRowDto>>.Success(rows);
    }

    public async Task<Result<FilePayloadDto>> ExportAsync(string reportType, string format, CancellationToken cancellationToken)
    {
        var normalizedReport = reportType.Trim().ToLowerInvariant();
        var normalizedFormat = format.Trim().ToLowerInvariant();

        if (normalizedFormat is not ("pdf" or "xlsx"))
        {
            return Result<FilePayloadDto>.Failure("validation_error", "format must be pdf or xlsx.");
        }

        var headers = new List<string>();
        var rows = new List<IReadOnlyList<string>>();
        var title = "Report";

        if (normalizedReport == "transfers")
        {
            var result = await GetTransfersReportAsync(cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return Result<FilePayloadDto>.Failure("report_error", "Unable to build transfer report.");
            }

            headers = ["TransferId", "StudentCode", "StudentName", "FromCampus", "ToCampus", "Shift", "Status", "CreatedAt"];
            rows = result.Value.Select(x => (IReadOnlyList<string>)
            [
                x.TransferId.ToString(),
                x.StudentCode,
                x.StudentName,
                x.FromCampus,
                x.ToCampus,
                x.Shift,
                x.Status,
                x.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            ]).ToList();
            title = "Reporte de Traslados";
        }
        else if (normalizedReport == "enrollments")
        {
            var result = await GetEnrollmentsReportAsync(cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return Result<FilePayloadDto>.Failure("report_error", "Unable to build enrollment report.");
            }

            headers = ["EnrollmentId", "StudentCode", "StudentName", "Type", "Status", "TotalAmount", "CreatedAt"];
            rows = result.Value.Select(x => (IReadOnlyList<string>)
            [
                x.EnrollmentId.ToString(),
                x.StudentCode,
                x.StudentName,
                x.EnrollmentType,
                x.Status,
                x.TotalAmount.ToString("0.00"),
                x.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            ]).ToList();
            title = "Reporte de Asignaciones";
        }
        else if (normalizedReport == "certificates")
        {
            var result = await GetCertificatesReportAsync(cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return Result<FilePayloadDto>.Failure("report_error", "Unable to build certificates report.");
            }

            headers = ["CertificateId", "StudentCode", "StudentName", "Status", "VerificationCode", "CreatedAt", "GeneratedAt"];
            rows = result.Value.Select(x => (IReadOnlyList<string>)
            [
                x.CertificateId.ToString(),
                x.StudentCode,
                x.StudentName,
                x.Status,
                x.VerificationCode,
                x.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                x.GeneratedAt?.ToString("yyyy-MM-dd HH:mm") ?? ""
            ]).ToList();
            title = "Reporte de Certificaciones";
        }
        else
        {
            return Result<FilePayloadDto>.Failure("validation_error", "Unknown report type.");
        }

        if (normalizedFormat == "pdf")
        {
            var content = _pdfService.BuildTableReportPdf(title, headers, rows);
            return Result<FilePayloadDto>.Success(new FilePayloadDto(content, "application/pdf", $"{normalizedReport}-report.pdf"));
        }

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Report");
        for (var i = 0; i < headers.Count; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            for (var col = 0; col < row.Count; col++)
            {
                sheet.Cell(rowIndex + 2, col + 1).Value = row[col];
            }
        }

        sheet.Columns().AdjustToContents();

        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result<FilePayloadDto>.Success(new FilePayloadDto(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{normalizedReport}-report.xlsx"));
    }

    private async Task<HashSet<int>> GetOverdueIdsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var failed = await BuildFailedHistoryQuery(_dbContext.StudentCourseHistories, studentId)
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var passed = await BuildApprovedHistoryQuery(_dbContext.StudentCourseHistories, studentId)
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        failed.RemoveAll(courseId => passed.Contains(courseId));
        return failed.ToHashSet();
    }

    private async Task<ServicePricing?> GetServicePricingAsync(string serviceType, int? programId, CancellationToken cancellationToken)
    {
        var price = await _dbContext.PricingCatalogs
            .AsNoTracking()
            .Where(x => x.ServiceType == serviceType && x.IsActive && x.ProgramId == programId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ServicePricing(x.Amount, x.Currency))
            .FirstOrDefaultAsync(cancellationToken);

        if (price is not null)
        {
            return price;
        }

        return await _dbContext.PricingCatalogs
            .AsNoTracking()
            .Where(x => x.ServiceType == serviceType && x.IsActive && x.ProgramId == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ServicePricing(x.Amount, x.Currency))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<int> ReconcileExpiredPendingAsync(Guid? studentId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var createdCutoff = now.AddHours(-_academicOptions.PendingPaymentExpirationHours);

        var query = _dbContext.PaymentOrders.Where(x =>
            x.Status == DomainStatuses.Payment.Pending &&
            (x.ExpiresAt <= now ||
             x.CreatedAt <= createdCutoff));

        if (studentId.HasValue)
        {
            query = query.Where(x => x.StudentId == studentId.Value);
        }

        var expiredPayments = await query.ToListAsync(cancellationToken);
        if (expiredPayments.Count == 0)
        {
            return 0;
        }

        foreach (var payment in expiredPayments)
        {
            var expectedExpiry = payment.CreatedAt.AddHours(_academicOptions.PendingPaymentExpirationHours);
            if (payment.ExpiresAt > expectedExpiry)
            {
                payment.ExpiresAt = expectedExpiry;
            }

            payment.Status = DomainStatuses.Payment.Cancelled;
            payment.CancelledAt = now;
        }

        var transferIds = expiredPayments
            .Where(x => x.OrderType == "Transfer")
            .Select(x => x.ReferenceId)
            .Distinct()
            .ToList();

        var enrollmentIds = expiredPayments
            .Where(x => x.OrderType == "Enrollment")
            .Select(x => x.ReferenceId)
            .Distinct()
            .ToList();

        var certificateIds = expiredPayments
            .Where(x => x.OrderType == "Certificate")
            .Select(x => x.ReferenceId)
            .Distinct()
            .ToList();

        if (transferIds.Count > 0)
        {
            var transfers = await _dbContext.TransferRequests
                .Where(x => transferIds.Contains(x.Id) &&
                            (x.Status == DomainStatuses.Transfer.PendingPayment || x.Status == DomainStatuses.Transfer.PendingReview))
                .ToListAsync(cancellationToken);

            foreach (var transfer in transfers)
            {
                transfer.Status = DomainStatuses.Transfer.Cancelled;
                transfer.UpdatedAt = now;
                transfer.ReviewNotes = string.IsNullOrWhiteSpace(transfer.ReviewNotes)
                    ? "Auto-cancelled due to expired payment."
                    : transfer.ReviewNotes + " | Auto-cancelled due to expired payment.";
            }
        }

        if (enrollmentIds.Count > 0)
        {
            var enrollments = await _dbContext.Enrollments
                .Where(x => enrollmentIds.Contains(x.Id) && x.Status == DomainStatuses.Enrollment.PendingPayment)
                .ToListAsync(cancellationToken);

            foreach (var enrollment in enrollments)
            {
                enrollment.Status = DomainStatuses.Enrollment.Cancelled;
                enrollment.UpdatedAt = now;
            }
        }

        if (certificateIds.Count > 0)
        {
            var certificates = await _dbContext.Certificates
                .Where(x => certificateIds.Contains(x.Id) &&
                            (x.Status == DomainStatuses.Certificate.Requested || x.Status == DomainStatuses.Certificate.PdfGenerated))
                .ToListAsync(cancellationToken);

            foreach (var certificate in certificates)
            {
                certificate.Status = DomainStatuses.Certificate.Cancelled;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null,
            "ExpiredPendingReconciled",
            "PaymentOrder",
            studentId?.ToString() ?? "all",
            new { Count = expiredPayments.Count },
            null,
            cancellationToken);

        return expiredPayments.Count;
    }

    private async Task<Result<PaymentOrderDto>> MarkPaymentAsPaidInternalAsync(
        PaymentOrder payment,
        Guid actedByUserId,
        CancellationToken cancellationToken)
    {
        await ReconcileExpiredPendingAsync(payment.StudentId, cancellationToken);

        if (payment.Status != DomainStatuses.Payment.Pending)
        {
            return Result<PaymentOrderDto>.Failure("business_rule", "Only pending payments can be marked as paid.");
        }

        if (IsPaymentExpired(payment))
        {
            payment.Status = DomainStatuses.Payment.Cancelled;
            payment.CancelledAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<PaymentOrderDto>.Failure("business_rule", "Payment order expired and was cancelled.");
        }

        payment.Status = DomainStatuses.Payment.Paid;
        payment.PaidAt = DateTime.UtcNow;

        if (payment.OrderType == "Transfer")
        {
            var transfer = await _dbContext.TransferRequests.FirstOrDefaultAsync(x => x.Id == payment.ReferenceId, cancellationToken);
            if (transfer is not null)
            {
                transfer.Status = DomainStatuses.Transfer.PendingReview;
                transfer.UpdatedAt = DateTime.UtcNow;
            }
        }
        else if (payment.OrderType == "Enrollment")
        {
            var enrollment = await _dbContext.Enrollments.FirstOrDefaultAsync(x => x.Id == payment.ReferenceId, cancellationToken);
            if (enrollment is not null)
            {
                enrollment.Status = DomainStatuses.Enrollment.Confirmed;
                enrollment.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (payment.OrderType == "Enrollment")
        {
            var direGeneration = await EnsureEnrollmentDireGeneratedAsync(payment.ReferenceId, payment.StudentId, cancellationToken);
            if (!direGeneration.IsSuccess)
            {
                _logger.LogWarning(
                    "Payment {PaymentId} was marked as paid, but DIRE generation failed: {Reason}",
                    payment.Id,
                    direGeneration.Error?.Message ?? "Unknown error");
            }
        }

        await _auditLogService.LogAsync(
            actedByUserId,
            "PaymentMarkedPaid",
            "PaymentOrder",
            payment.Id.ToString(),
            new { payment.OrderType, payment.ReferenceId },
            null,
            cancellationToken);

        return Result<PaymentOrderDto>.Success(ToPaymentOrderDto(payment));
    }

    private async Task<Result<MockCheckoutCertificateDto>> TryGenerateCertificateFromPaymentAsync(
        Guid studentId,
        Guid paymentOrderId,
        CancellationToken cancellationToken)
    {
        var certificate = await _dbContext.Certificates
            .Include(x => x.PaymentOrder)
            .FirstOrDefaultAsync(x => x.PaymentOrderId == paymentOrderId, cancellationToken);

        if (certificate is null)
        {
            return Result<MockCheckoutCertificateDto>.Failure("not_found", "No se encontró la certificación asociada al pago.");
        }

        if (certificate.StudentId != studentId)
        {
            return Result<MockCheckoutCertificateDto>.Failure("forbidden", "La certificación no pertenece al estudiante actual.");
        }

        if (certificate.PaymentOrder.Status != DomainStatuses.Payment.Paid)
        {
            return Result<MockCheckoutCertificateDto>.Failure("business_rule", "La orden de certificación aún no está pagada.");
        }

        if (certificate.Status == DomainStatuses.Certificate.Cancelled)
        {
            return Result<MockCheckoutCertificateDto>.Failure("business_rule", "La certificación está cancelada.");
        }

        if (certificate.Status == DomainStatuses.Certificate.Requested ||
            string.IsNullOrWhiteSpace(certificate.PdfPath) ||
            !File.Exists(certificate.PdfPath))
        {
            var generateResult = await GenerateAsync(studentId, certificate.Id, new GenerateCertificateDto(false, false), cancellationToken);
            if (!generateResult.IsSuccess || generateResult.Value is null)
            {
                return Result<MockCheckoutCertificateDto>.Failure(
                    generateResult.Error?.Code ?? "certificate_error",
                    generateResult.Error?.Message ?? "No se pudo generar el certificado.");
            }

            return Result<MockCheckoutCertificateDto>.Success(new MockCheckoutCertificateDto(
                certificate.Id,
                generateResult.Value.Status,
                generateResult.Value.VerificationCode,
                !string.IsNullOrWhiteSpace(generateResult.Value.PdfPath) && File.Exists(generateResult.Value.PdfPath)));
        }

        return Result<MockCheckoutCertificateDto>.Success(new MockCheckoutCertificateDto(
            certificate.Id,
            certificate.Status,
            certificate.VerificationCode,
            !string.IsNullOrWhiteSpace(certificate.PdfPath) && File.Exists(certificate.PdfPath)));
    }

    private async Task<Result<EnrollmentDireArtifact>> EnsureEnrollmentDireGeneratedAsync(
        Guid enrollmentId,
        Guid? studentId,
        CancellationToken cancellationToken)
    {
        var enrollment = await _dbContext.Enrollments
            .AsNoTracking()
            .Include(x => x.Student)
                .ThenInclude(x => x.Program)
            .Include(x => x.Student)
                .ThenInclude(x => x.CurrentCampus)
            .Include(x => x.Student)
                .ThenInclude(x => x.CurrentShift)
            .Include(x => x.EnrollmentCourses)
                .ThenInclude(x => x.Course)
            .Include(x => x.EnrollmentCourses)
                .ThenInclude(x => x.Shift)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, cancellationToken);

        if (enrollment is null)
        {
            return Result<EnrollmentDireArtifact>.Failure("not_found", "No se encontró la asignación.");
        }

        if (studentId.HasValue && enrollment.StudentId != studentId.Value)
        {
            return Result<EnrollmentDireArtifact>.Failure("forbidden", "La asignación no pertenece al estudiante actual.");
        }

        if (enrollment.Status != DomainStatuses.Enrollment.Confirmed)
        {
            return Result<EnrollmentDireArtifact>.Failure("business_rule", "El DIRE se genera cuando la asignación está confirmada.");
        }

        var paymentOrder = await _dbContext.PaymentOrders
            .AsNoTracking()
            .Where(x => x.OrderType == "Enrollment" && x.ReferenceId == enrollmentId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (paymentOrder is null || paymentOrder.Status != DomainStatuses.Payment.Paid)
        {
            return Result<EnrollmentDireArtifact>.Failure("business_rule", "No existe pago confirmado para la asignación.");
        }

        var artifact = BuildEnrollmentDireArtifact(enrollment);
        if (File.Exists(artifact.FullPath))
        {
            return Result<EnrollmentDireArtifact>.Success(artifact);
        }

        var model = new EnrollmentDirePdfModel(
            artifact.DireNumber,
            DateTime.UtcNow,
            $"{enrollment.Student.FirstName} {enrollment.Student.LastName}".Trim(),
            enrollment.Student.Carnet,
            enrollment.Student.StudentCode,
            enrollment.Student.Program.Name,
            enrollment.Student.CurrentCampus?.Name ?? "Sede no definida",
            ToShiftLabel(enrollment.Student.CurrentShift?.Name),
            enrollment.EnrollmentType,
            enrollment.TotalAmount,
            paymentOrder.Currency,
            enrollment.EnrollmentCourses
                .OrderBy(x => x.Course.Code)
                .Select(x => new EnrollmentDireCourseLine(
                    x.Course.Code,
                    x.Course.Name,
                    ToShiftLabel(x.Shift.Name),
                    x.IsOverdue ? "Atrasado" : "Regular"))
                .ToList());

        var bytes = _pdfService.BuildEnrollmentDirePdf(model);
        await File.WriteAllBytesAsync(artifact.FullPath, bytes, cancellationToken);

        await _auditLogService.LogAsync(
            enrollment.Student.UserId,
            "EnrollmentDireGenerated",
            "Enrollment",
            enrollment.Id.ToString(),
            new { artifact.DireNumber, artifact.FileName },
            null,
            cancellationToken);

        return Result<EnrollmentDireArtifact>.Success(artifact);
    }

    private static Dictionary<string, string[]> ValidateMockCard(MockCheckoutRequestDto request)
    {
        var errors = new Dictionary<string, List<string>>();

        var holderName = request.CardHolderName?.Trim() ?? string.Empty;
        if (holderName.Length < 4)
        {
            AddValidationError(errors, "cardHolderName", "Nombre del titular inválido.");
        }

        var cardDigits = new string((request.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cardDigits.Length < 13 || cardDigits.Length > 19)
        {
            AddValidationError(errors, "cardNumber", "Número de tarjeta inválido.");
        }
        else if (!PassesLuhn(cardDigits))
        {
            AddValidationError(errors, "cardNumber", "Número de tarjeta no válido.");
        }

        if (request.ExpiryMonth is < 1 or > 12)
        {
            AddValidationError(errors, "expiryMonth", "Mes de vencimiento inválido.");
        }

        if (request.ExpiryYear is < 2000 or > 2100)
        {
            AddValidationError(errors, "expiryYear", "Año de vencimiento inválido.");
        }
        else
        {
            var now = DateTime.UtcNow;
            if (request.ExpiryYear < now.Year ||
                (request.ExpiryYear == now.Year && request.ExpiryMonth < now.Month))
            {
                AddValidationError(errors, "expiryMonth", "La tarjeta está vencida.");
            }
        }

        var cvv = request.Cvv?.Trim() ?? string.Empty;
        if (cvv.Length is < 3 or > 4 || cvv.Any(character => !char.IsDigit(character)))
        {
            AddValidationError(errors, "cvv", "CVV inválido.");
        }

        return errors.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    private static void AddValidationError(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.TryGetValue(field, out var fieldErrors))
        {
            fieldErrors = [];
            errors[field] = fieldErrors;
        }

        fieldErrors.Add(message);
    }

    private static bool PassesLuhn(string cardDigits)
    {
        var sum = 0;
        var doubleDigit = false;
        for (var index = cardDigits.Length - 1; index >= 0; index--)
        {
            var digit = cardDigits[index] - '0';
            if (doubleDigit)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            sum += digit;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    private static string MaskCardNumber(string cardNumber)
    {
        var digits = new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length <= 4)
        {
            return "****";
        }

        return new string('*', Math.Max(0, digits.Length - 4)) + digits[^4..];
    }

    private static PaymentOrderDto ToPaymentOrderDto(PaymentOrder payment)
        => new(
            payment.Id,
            payment.OrderType,
            payment.ReferenceId,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.Description,
            payment.CreatedAt,
            payment.ExpiresAt,
            payment.PaidAt,
            payment.CancelledAt);

    private EnrollmentDireArtifact BuildEnrollmentDireArtifact(Enrollment enrollment)
    {
        var suffix = enrollment.Id.ToString("N")[..8].ToUpperInvariant();
        var sanitizedCarnet = new string((enrollment.Student.Carnet ?? string.Empty)
            .Where(character => char.IsLetterOrDigit(character) || character == '-')
            .ToArray());
        if (string.IsNullOrWhiteSpace(sanitizedCarnet))
        {
            sanitizedCarnet = enrollment.Student.StudentCode;
        }

        var direNumber = $"DIRE-{sanitizedCarnet}-{enrollment.CreatedAt:yyyy}-{suffix}";
        var fileName = $"{direNumber}.pdf";
        var outputDirectory = Path.Combine(_hostEnvironment.ContentRootPath, _academicOptions.EnrollmentDireStoragePath);
        Directory.CreateDirectory(outputDirectory);
        var fullPath = Path.Combine(outputDirectory, fileName);
        return new EnrollmentDireArtifact(direNumber, fileName, fullPath);
    }

    private static IQueryable<StudentCourseHistory> BuildApprovedHistoryQuery(
        IQueryable<StudentCourseHistory> source,
        Guid studentId)
        => source.Where(x =>
            x.StudentId == studentId &&
            x.Status != null &&
            ApprovedHistoryStatuses.Contains(x.Status.Trim().ToLower()));

    private static IQueryable<StudentCourseHistory> BuildFailedHistoryQuery(
        IQueryable<StudentCourseHistory> source,
        Guid studentId)
        => source.Where(x =>
            x.StudentId == studentId &&
            x.Status != null &&
            FailedHistoryStatuses.Contains(x.Status.Trim().ToLower()));

    private static bool IsPaymentExpired(PaymentOrder payment)
        => payment.ExpiresAt <= DateTime.UtcNow;

    private static string BuildPrerequisiteSummary(IReadOnlyCollection<string> courseCodes, IReadOnlyCollection<short> creditRequirements)
    {
        if (courseCodes.Count == 0 && creditRequirements.Count == 0)
        {
            return "Sin prerrequisitos";
        }

        var parts = new List<string>();
        if (courseCodes.Count > 0)
        {
            parts.Add($"Cursos: {string.Join(", ", courseCodes)}");
        }

        if (creditRequirements.Count > 0)
        {
            parts.Add($"Créditos aprobados: {string.Join(" / ", creditRequirements)}");
        }

        return string.Join(" | ", parts);
    }

    private static string? NormalizeModality(string value)
    {
        var normalized = RemoveDiacritics(value).Trim().ToLowerInvariant();
        return normalized switch
        {
            "presencial" => "Presencial",
            "virtual" => "Virtual",
            _ => null
        };
    }

    private static string NormalizeShift(string value)
    {
        var normalized = RemoveDiacritics(value).Trim().ToLowerInvariant();
        return normalized switch
        {
            "saturday" => "Saturday",
            "sabado" => "Saturday",
            "sunday" => "Sunday",
            "domingo" => "Sunday",
            _ => value.Trim()
        };
    }

    private static string ToShiftLabel(string? shiftName)
    {
        var normalized = NormalizeShift(shiftName ?? "Saturday");
        return normalized == "Sunday" ? "Domingo" : "Sabado";
    }

    private static CertificateTypeDefinition? ResolveCertificateType(string value)
    {
        var normalized = NormalizeForLookup(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized switch
        {
            "courses" => CertificateTypes.Single(x => x.Code == "courses"),
            "certificaciondecursos" => CertificateTypes.Single(x => x.Code == "courses"),
            "certificaciondecursosaprobados" => CertificateTypes.Single(x => x.Code == "courses"),
            "enrollment" => CertificateTypes.Single(x => x.Code == "enrollment"),
            "certificaciondematricula" => CertificateTypes.Single(x => x.Code == "enrollment"),
            "internships" => CertificateTypes.Single(x => x.Code == "internships"),
            "certificaciondepasantias" => CertificateTypes.Single(x => x.Code == "internships"),
            "pensumclosure" => CertificateTypes.Single(x => x.Code == "pensum-closure"),
            "pensum-closure" => CertificateTypes.Single(x => x.Code == "pensum-closure"),
            "cierrepensum" => CertificateTypes.Single(x => x.Code == "pensum-closure"),
            _ => null
        };
    }

    private static string NormalizeForLookup(string input)
    {
        var normalized = RemoveDiacritics(input).Trim().ToLowerInvariant();
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character) || character == '-')
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private static string RemoveDiacritics(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var normalized = input.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }

    private static string GenerateVerificationCode()
    {
        var random = Convert.ToHexString(Guid.NewGuid().ToByteArray())[..12];
        return $"CERT-{DateTime.UtcNow:yyyyMMdd}-{random}";
    }

    private sealed record EnrollmentDireArtifact(string DireNumber, string FileName, string FullPath);
    private sealed record ServicePricing(decimal Amount, string Currency);
}

