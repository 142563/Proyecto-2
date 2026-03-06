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
            shiftName));
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

        var coursesRaw = await _dbContext.Courses
            .AsNoTracking()
            .Include(c => c.Courses)
            .Include(c => c.CourseCreditRequirements)
            .Where(x => x.ProgramId == student.ProgramId && x.IsActive)
            .OrderBy(x => x.Cycle)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var courses = coursesRaw.Select(x =>
        {
            var prerequisiteCodes = x.Courses
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

        var uniqueIds = request.CourseIds.Where(x => x > 0).Distinct().ToList();
        if (uniqueIds.Count == 0)
        {
            return Result<EnrollmentResultDto>.ValidationFailure(new Dictionary<string, string[]>
            {
                ["courseIds"] = ["At least one valid course ID is required."]
            });
        }

        var student = await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

        if (student is null)
        {
            return Result<EnrollmentResultDto>.Failure("not_found", "Student profile not found.");
        }

        var hasPendingEnrollment = await _dbContext.Enrollments
            .AnyAsync(x => x.StudentId == studentId && x.Status == DomainStatuses.Enrollment.PendingPayment, cancellationToken);

        if (hasPendingEnrollment)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "You already have an active enrollment request.");
        }

        var selectedCourses = await _dbContext.Courses
            .Include(c => c.Courses)
            .Include(c => c.CourseCreditRequirements)
            .Where(x => uniqueIds.Contains(x.Id) && x.ProgramId == student.ProgramId && x.IsActive)
            .ToListAsync(cancellationToken);

        if (selectedCourses.Count != uniqueIds.Count)
        {
            return Result<EnrollmentResultDto>.Failure("business_rule", "One or more selected courses are invalid for your program.");
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
                return Result<EnrollmentResultDto>.Failure("business_rule", $"No seats available for course {course.Code}.");
            }
        }

        var passedCourseIds = await _dbContext.StudentCourseHistories
            .Where(x => x.StudentId == studentId && x.Status == "Passed")
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var approvedCredits = await _dbContext.Courses
            .Where(x => passedCourseIds.Contains(x.Id))
            .SumAsync(x => (int?)x.Credits, cancellationToken) ?? 0;

        foreach (var course in selectedCourses)
        {
            var prerequisiteIds = course.Courses.Select(x => x.Id).Distinct().ToList();
            var unmet = prerequisiteIds.Where(id => !passedCourseIds.Contains(id)).ToList();
            if (unmet.Count > 0)
            {
                return Result<EnrollmentResultDto>.Failure("business_rule", $"Prerequisites are not met for course {course.Code}.");
            }

            var minCredits = course.CourseCreditRequirements
                .Select(x => x.MinApprovedCredits)
                .DefaultIfEmpty((short)0)
                .Max();
            if (approvedCredits < minCredits)
            {
                return Result<EnrollmentResultDto>.Failure(
                    "business_rule",
                    $"No cumples el mínimo de créditos aprobados para {course.Code}. Requiere {minCredits}.");
            }
        }

        var overdueIds = await GetOverdueIdsAsync(studentId, cancellationToken);

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
                courses = uniqueIds
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

        await _auditLogService.LogAsync(
            actedByUserId,
            "PaymentMarkedPaid",
            "PaymentOrder",
            payment.Id.ToString(),
            new { payment.OrderType, payment.ReferenceId },
            null,
            cancellationToken);

        return Result<PaymentOrderDto>.Success(new PaymentOrderDto(
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
            payment.CancelledAt));
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
            return Result<CertificateCreatedDto>.Failure("business_rule", "An active certificate request already exists.");
        }

        var certificatePricing = await GetServicePricingAsync("Certificate", student.ProgramId, cancellationToken);
        if (certificatePricing is null || certificatePricing.Amount <= 0)
        {
            return Result<CertificateCreatedDto>.Failure("config_error", "Certificate pricing is not configured.");
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
            Description = "Pago certificacion digital",
            CreatedAt = now,
            ExpiresAt = now.AddHours(_academicOptions.PendingPaymentExpirationHours)
        };

        var certificate = new Certificate
        {
            Id = certificateId,
            StudentId = studentId,
            PaymentOrderId = paymentId,
            Purpose = request.Purpose.Trim(),
            Status = DomainStatuses.Certificate.Requested,
            VerificationCode = verificationCode,
            CreatedAt = now,
            Metadata = JsonSerializer.Serialize(new { source = "api", requestedBy = student.User.Email })
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

        var approvedCourses = await _dbContext.StudentCourseHistories
            .AsNoTracking()
            .Where(x => x.StudentId == actorStudentId && x.Status == "Passed")
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
        var failed = await _dbContext.StudentCourseHistories
            .Where(x => x.StudentId == studentId && x.Status == "Failed")
            .Select(x => x.CourseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var passed = await _dbContext.StudentCourseHistories
            .Where(x => x.StudentId == studentId && x.Status == "Passed")
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

    private sealed record ServicePricing(decimal Amount, string Currency);
}

