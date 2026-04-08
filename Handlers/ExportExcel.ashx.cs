using Newtonsoft.Json;
using SafetyCompliance.Models;
using SafetyCompliance.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SafetyCompliance.Handlers
{
    public class ExportExcel : HttpTaskAsyncHandler
    {
        private readonly IComplianceRepository _repo;

        public ExportExcel()
        {
            _repo = new ComplianceRepository();
        }

        public ExportExcel(IComplianceRepository repo)
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

                if (audit == null || audit.Gaps == null || audit.Gaps.Count == 0)
                {
                    context.Response.Write("No data found for Excel export.");
                    return;
                }

                context.Response.Clear();
                context.Response.Buffer = true;
                context.Response.AddHeader("content-disposition", $"attachment;filename=SafetyAudit_{auditId}.csv");
                context.Response.ContentType = "text/csv";
                context.Response.ContentEncoding = Encoding.UTF8;

                byte[] bom = Encoding.UTF8.GetPreamble();
                context.Response.BinaryWrite(bom);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Domain,Finding,Severity,Deadline,Recommendation");

                foreach (var gap in audit.Gaps)
                {
                    sb.AppendLine($"{CleanForCsv(gap.Domain)},{CleanForCsv(gap.Finding)},{CleanForCsv(gap.Severity)},{CleanForCsv(gap.Deadline)},{CleanForCsv(gap.Recommendation)}");
                }

                await context.Response.Output.WriteAsync(sb.ToString());
                context.Response.Flush();
            }
            catch (Exception ex)
            {
                context.Response.Write("Error exporting Excel: " + ex.Message);
            }
        }

        private string CleanForCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return "N/A";

            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n") || input.Contains("\r"))
            {
                string cleaned = input.Replace("\r", " ").Replace("\n", " ").Replace("\"", "\"\"");
                return $"\"{cleaned}\"";
            }
            return input;
        }
    }
}