using iTextSharp.text;
using iTextSharp.text.pdf;
using SafetyCompliance.Models;
using SafetyCompliance.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace SafetyCompliance.Handlers
{
    public class ExportPdf : HttpTaskAsyncHandler
    {
        private readonly IComplianceRepository _repo;

        public ExportPdf()
        {
            _repo = new ComplianceRepository();
        }

        public ExportPdf(IComplianceRepository repo)
        {
            _repo = repo;
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            if (!int.TryParse(context.Request.QueryString["auditId"], out int auditId))
            {
                context.Response.Write("Invalid Audit ID.");
                return;
            }

            try
            {
                var audit = await _repo.GetAuditResultByIdAsync(auditId);

                if (audit == null || audit.Gaps == null)
                {
                    context.Response.Write("Audit result not found in database.");
                    return;
                }

                context.Response.Clear();
                context.Response.ContentType = "application/pdf";
                context.Response.AddHeader("content-disposition", $"attachment;filename=SafetyAudit_{auditId}.pdf");

                using (var ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);

                    document.Open();

                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    document.Add(new Paragraph($"Safety Compliance Audit Report #{auditId}", titleFont));
                    document.Add(new Paragraph($"Overall Readiness Score: {audit.ReadinessScore}%"));
                    document.Add(new Paragraph($"Scan Date: {audit.ScanDate:yyyy-MM-dd HH:mm}"));
                    document.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 15f, 25f, 10f, 15f, 35f });

                    AddCell(table, "Domain", true);
                    AddCell(table, "Finding", true);
                    AddCell(table, "Severity", true);
                    AddCell(table, "Deadline", true);
                    AddCell(table, "Recommendation", true);

                    foreach (var gap in audit.Gaps)
                    {
                        AddCell(table, gap.Domain);
                        AddCell(table, gap.Finding);
                        AddCell(table, gap.Severity);
                        AddCell(table, gap.Deadline);
                        AddCell(table, gap.Recommendation);
                    }

                    document.Add(table);
                    document.Close();

                    byte[] bytes = ms.ToArray();
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                }

                context.Response.Flush();
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error generating PDF: " + ex.Message);
            }
        }

        private void AddCell(PdfPTable table, string text, bool isHeader = false)
        {
            var cell = new PdfPCell(new Phrase(text ?? "N/A",
                isHeader ? FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10) : FontFactory.GetFont(FontFactory.HELVETICA, 9)));

            if (isHeader) cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell.Padding = 5;
            table.AddCell(cell);
        }
    }
}