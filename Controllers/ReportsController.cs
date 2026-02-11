using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visionguard.Data;
using visionguard.DTOs;
using visionguard.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;

namespace visionguard.Controllers
{
    /// <summary>
    /// Reports Controller — serves aggregated violation data for reporting
    /// Accessible by both SAFETY_SUPERVISOR and HR roles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly VisionGuardDbContext _context;

        public ReportsController(VisionGuardDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// POST /api/reports/violations-by-worker
        /// Returns aggregated violations grouped by worker for the given period
        /// </summary>
        [HttpPost("violations-by-worker")]
        public async Task<IActionResult> GetViolationsByWorker([FromBody] ReportFilterRequest filter)
        {
            var periodStart = new DateTime(filter.Year, filter.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

            var query = _context.Violations
                .Include(v => v.Worker)
                .Include(v => v.Camera)
                .Where(v => v.DetectedAt >= periodStart && v.DetectedAt <= periodEnd);

            if (!string.IsNullOrEmpty(filter.CameraZone))
                query = query.Where(v => v.Camera!.Zone == filter.CameraZone);

            if (!string.IsNullOrEmpty(filter.ViolationType) && Enum.TryParse<PPEType>(filter.ViolationType, true, out var ppeType))
                query = query.Where(v => v.ViolationType == ppeType);

            // Group by worker and compute breakdown
            var result = await query
                .GroupBy(v => new { v.WorkerId, v.Worker!.Name, v.Worker.EmployeeId })
                .Select(g => new WorkerViolationReportDto
                {
                    WorkerId = g.Key.WorkerId,
                    WorkerName = g.Key.Name,
                    EmployeeId = g.Key.EmployeeId,
                    TotalViolations = g.Count(),
                    Breakdown = g.GroupBy(v => v.ViolationType)
                                 .Select(tg => new ViolationTypeCountDto
                                 {
                                     Type = tg.Key.ToString(),
                                     Count = tg.Count()
                                 })
                                 .ToList()
                })
                .OrderByDescending(w => w.TotalViolations)
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// POST /api/reports/violations-by-type
        /// Returns violations grouped by PPE type
        /// </summary>
        [HttpPost("violations-by-type")]
        public async Task<IActionResult> GetViolationsByType([FromBody] ReportFilterRequest filter)
        {
            var periodStart = new DateTime(filter.Year, filter.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

            var query = _context.Violations
                .Include(v => v.Camera)
                .Where(v => v.DetectedAt >= periodStart && v.DetectedAt <= periodEnd);

            if (!string.IsNullOrEmpty(filter.CameraZone))
                query = query.Where(v => v.Camera!.Zone == filter.CameraZone);

            var result = await query
                .GroupBy(v => v.ViolationType)
                .Select(g => new ViolationTypeCountDto
                {
                    Type = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// GET /api/reports/monthly-summary
        /// Returns high-level monthly metrics
        /// </summary>
        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

            var violations = await _context.Violations
                .Where(v => v.DetectedAt >= periodStart && v.DetectedAt <= periodEnd)
                .ToListAsync();

            var breakdown = violations
                .GroupBy(v => v.ViolationType)
                .Select(g => new ViolationTypeCountDto
                {
                    Type = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            var summary = new MonthlySummaryResponseDto
            {
                Month = periodStart.ToString("yyyy-MM"),
                TotalViolations = violations.Count,
                Breakdown = breakdown
            };

            return Ok(summary);
        }

        /// <summary>
        /// POST /api/reports/export-excel
        /// Placeholder for Excel export
        /// </summary>
        /// <summary>
        /// POST /api/reports/export-excel
        /// Generates an Excel report of violations
        /// </summary>
        [HttpPost("export-excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] ReportFilterRequest filter)
        {
            var periodStart = new DateTime(filter.Year, filter.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

            var query = _context.Violations
                .Include(v => v.Worker)
                .Include(v => v.Camera)
                .Where(v => v.DetectedAt >= periodStart && v.DetectedAt <= periodEnd);

            if (!string.IsNullOrEmpty(filter.CameraZone))
                query = query.Where(v => v.Camera!.Zone == filter.CameraZone);
            
            if (!string.IsNullOrEmpty(filter.ViolationType) && Enum.TryParse<PPEType>(filter.ViolationType, true, out var ppeType))
                query = query.Where(v => v.ViolationType == ppeType);

            var violations = await query.ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Violations");

            // Header
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Worker";
            worksheet.Cell(1, 3).Value = "Violation Type";
            worksheet.Cell(1, 4).Value = "Camera Zone";
            worksheet.Cell(1, 5).Value = "Detected At";
            worksheet.Cell(1, 6).Value = "Confidence Score";

            // Data
            for (int i = 0; i < violations.Count; i++)
            {
                var v = violations[i];
                worksheet.Cell(i + 2, 1).Value = v.Id;
                worksheet.Cell(i + 2, 2).Value = v.Worker?.Name ?? "Unknown";
                worksheet.Cell(i + 2, 3).Value = v.ViolationType.ToString();
                worksheet.Cell(i + 2, 4).Value = v.Camera?.Zone ?? "Unknown";
                worksheet.Cell(i + 2, 5).Value = v.DetectedAt.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(i + 2, 6).Value = $"{v.ConfidenceScore}%";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Violations_{filter.Year}_{filter.Month}.xlsx");
        }

        /// <summary>
        /// POST /api/reports/export-pdf
        /// Generates a PDF report of violations
        /// </summary>
        [HttpPost("export-pdf")]
        public async Task<IActionResult> ExportToPdf([FromBody] ReportFilterRequest filter)
        {
            var periodStart = new DateTime(filter.Year, filter.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

            var query = _context.Violations
                .Include(v => v.Worker)
                .Include(v => v.Camera)
                .Where(v => v.DetectedAt >= periodStart && v.DetectedAt <= periodEnd);

            if (!string.IsNullOrEmpty(filter.CameraZone))
                query = query.Where(v => v.Camera!.Zone == filter.CameraZone);
            
             if (!string.IsNullOrEmpty(filter.ViolationType) && Enum.TryParse<PPEType>(filter.ViolationType, true, out var ppeType))
                query = query.Where(v => v.ViolationType == ppeType);

            var violations = await query.ToListAsync();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"VisionGuard Violation Report - {filter.Year}-{filter.Month:00}")
                        .SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(20)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80); // Date
                                columns.RelativeColumn();   // Worker
                                columns.ConstantColumn(80); // Type
                                columns.RelativeColumn();   // Zone
                                columns.ConstantColumn(50); // Score
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date");
                                header.Cell().Element(CellStyle).Text("Worker");
                                header.Cell().Element(CellStyle).Text("Type");
                                header.Cell().Element(CellStyle).Text("Zone");
                                header.Cell().Element(CellStyle).Text("Score");

                                static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                                }
                            });

                            foreach (var v in violations)
                            {
                                table.Cell().Element(CellStyle).Text(v.DetectedAt.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(v.Worker?.Name ?? "Unknown");
                                table.Cell().Element(CellStyle).Text(v.ViolationType.ToString());
                                table.Cell().Element(CellStyle).Text(v.Camera?.Zone ?? "Unknown");
                                table.Cell().Element(CellStyle).Text($"{v.ConfidenceScore}%");

                                static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).PaddingVertical(3);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0; // Reset position for reading

            return File(stream, "application/pdf", $"Violations_{filter.Year}_{filter.Month}.pdf");
        }
    }
}
