using Academic.Application.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Academic.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] BuildCertificatePdf(CertificatePdfModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.Header().Column(header =>
                {
                    header.Item().Text("Universidad - Certificacion Digital").FontSize(20).Bold();
                    header.Item().Text($"Codigo de verificacion: {model.VerificationCode}").FontSize(11).FontColor(Colors.Grey.Darken2);
                    header.Item().Text($"Fecha: {model.GeneratedAt:yyyy-MM-dd HH:mm} UTC").FontSize(10);
                });

                page.Content().Column(content =>
                {
                    content.Spacing(8);
                    content.Item().Text($"Estudiante: {model.StudentName}").FontSize(13);
                    content.Item().Text($"Codigo estudiante: {model.StudentCode}").FontSize(12);
                    content.Item().Text($"Programa: {model.ProgramName}").FontSize(12);
                    content.Item().Text($"Motivo: {model.Purpose}").FontSize(12);

                    content.Item().PaddingTop(10).Text("Cursos aprobados:").FontSize(12).Bold();
                    foreach (var course in model.ApprovedCourses)
                    {
                        content.Item().Text($"- {course}").FontSize(11);
                    }

                    if (model.IncludeQr)
                    {
                        content.Item().PaddingTop(10).Text("QR opcional habilitado para verificacion (placeholder).")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                page.Footer().AlignCenter().Text("Documento generado automaticamente.").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();
    }

    public byte[] BuildEnrollmentDirePdf(EnrollmentDirePdfModel model)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Header().Column(header =>
                {
                    header.Item().Text("UNIVERSIDAD MARIANO GALVEZ DE GUATEMALA").FontSize(14).Bold();
                    header.Item().Text("DIRE DE INSCRIPCION").FontSize(20).Bold();
                    header.Item().Text($"No. DIRE: {model.DireNumber}").FontSize(11).Bold();
                    header.Item().Text($"Fecha de emision: {model.GeneratedAt:yyyy-MM-dd HH:mm}").FontSize(10);
                });

                page.Content().Column(content =>
                {
                    content.Spacing(8);
                    content.Item().Text($"Estudiante: {model.StudentName}").FontSize(11);
                    content.Item().Text($"Carnet: {model.Carnet}").FontSize(11);
                    content.Item().Text($"Codigo estudiante: {model.StudentCode}").FontSize(11);
                    content.Item().Text($"Programa: {model.ProgramName}").FontSize(11);
                    content.Item().Text($"Sede actual: {model.CampusName}").FontSize(11);
                    content.Item().Text($"Plan: {model.PlanShiftName}").FontSize(11);
                    content.Item().Text($"Tipo de asignacion: {model.EnrollmentType}").FontSize(11);
                    content.Item().Text($"Monto pagado: Q{model.TotalAmount:0.00} {model.Currency}").FontSize(11).Bold();

                    content.Item().PaddingTop(8).Text("Detalle de cursos asignados").FontSize(12).Bold();
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(DireCellStyle).Text("Codigo").Bold();
                            header.Cell().Element(DireCellStyle).Text("Curso").Bold();
                            header.Cell().Element(DireCellStyle).Text("Jornada").Bold();
                            header.Cell().Element(DireCellStyle).Text("Tipo").Bold();
                        });

                        foreach (var course in model.Courses)
                        {
                            table.Cell().Element(DireCellStyle).Text(course.CourseCode);
                            table.Cell().Element(DireCellStyle).Text(course.CourseName);
                            table.Cell().Element(DireCellStyle).Text(course.ShiftName);
                            table.Cell().Element(DireCellStyle).Text(course.CourseType);
                        }
                    });
                });

                page.Footer().AlignCenter().Text("Documento generado automaticamente por el portal estudiantil UMG.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();

        static IContainer DireCellStyle(IContainer container)
            => container.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(4);
    }

    public byte[] BuildTableReportPdf(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Header().Text(title).FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        foreach (var _ in headers)
                        {
                            c.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var headerCell in headers)
                        {
                            header.Cell().Element(CellStyle).Text(headerCell).Bold();
                        }
                    });

                    foreach (var row in rows)
                    {
                        foreach (var cell in row)
                        {
                            table.Cell().Element(CellStyle).Text(cell);
                        }
                    }
                });

                page.Footer().AlignRight().Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(9);
            });
        }).GeneratePdf();

        static IContainer CellStyle(IContainer container)
            => container.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(4);
    }
}
