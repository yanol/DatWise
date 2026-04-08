using SafetyCompliance.Models;
using System;
using System.Text;

namespace SafetyCompliance.Services
{
    /// <summary>
    /// Converts ComplianceData (raw SQL results) into a
    /// structured text prompt for AI.
    /// </summary>
    
    public static class PromptBuilder
    {
        public static string Build(ComplianceData data)
        {
            var sb = new StringBuilder();
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            sb.AppendLine("Analyze the following compliance data and return ONLY a JSON object matching the 'AuditResult' class structure.");
            sb.AppendLine($"IMPORTANT: Today's date is {currentDate}. Use this date to calculate how overdue an item is and to set realistic 'Deadline' dates." +

            "Structure requirements:" +
                "1. \"Gaps\": A list of objects. Each object MUST have these exact keys: " +
                "\"Domain\", \"Finding\", \"Severity\" (use only: critical, medium, low), \"Deadline\", \"Recommendation\"." +
                "2. \"ReadinessScore\": An integer from 0-100." +
                "3. \"Summary\": A brief text overview." +
                "4. \"CriticalCount\", \"MediumCount\", \"LowCount\": Integers reflecting the number of gaps per severity." +
                
                "Data to analyze:");

            sb.AppendLine($"Audit scope: {data.AuditType}");
            sb.AppendLine();

            // ── Training ──
            sb.AppendLine("=== TRAINING RECORDS (expired or expiring within 30 days) ===");
            if (data.TrainingGaps.Count == 0)
                sb.AppendLine("No issues found.");
            else
                foreach (var t in data.TrainingGaps)
                    sb.AppendLine(
                        $"- Employee: {t.Employee} | Course: {t.CourseName} | Expiry: {t.ExpiryDate:yyyy-MM-dd}");

            sb.AppendLine();

            // ── Incidents ───
            sb.AppendLine("=== OPEN INCIDENTS (no corrective action assigned) ===");
            if (data.OpenIncidents.Count == 0)
                sb.AppendLine("No issues found.");
            else
                foreach (var i in data.OpenIncidents)
                    sb.AppendLine(
                        $"- Date: {i.Date:yyyy-MM-dd} | Location: {i.Location} | Description: {i.Description}");

            sb.AppendLine();

            // ── Equipment ──
            sb.AppendLine("=== EQUIPMENT INSPECTIONS (overdue) ===");
            if (data.EquipmentGaps.Count == 0)
                sb.AppendLine("No issues found.");
            else
                foreach (var e in data.EquipmentGaps)
                    sb.AppendLine(
                        $"- Equipment: {e.Equipment} | Last checked: {e.LastInspection:yyyy-MM-dd} | Days overdue: {e.DaysOverdue}");

            sb.AppendLine();

            // ── Permits ───
            sb.AppendLine("=== PERMITS & LICENSES (expired or expiring within 60 days) ===");
            if (data.PermitGaps.Count == 0)
                sb.AppendLine("No issues found.");
            else
                foreach (var p in data.PermitGaps)
                    sb.AppendLine(
                        $"- Type: {p.PermitType} | Holder: {p.HolderName} | Expiry: {p.ExpiryDate:yyyy-MM-dd}");

            sb.AppendLine();

            // ── Drills ──
            sb.AppendLine("=== EMERGENCY DRILLS (overdue) ===");
            if (data.DrillGaps.Count == 0)
                sb.AppendLine("No issues found.");
            else
                foreach (var d in data.DrillGaps)
                    sb.AppendLine(
                        $"- Drill: {d.DrillType} | Last conducted: {d.LastDrillDate:yyyy-MM-dd} | Months overdue: {d.MonthsOverdue}");

            sb.AppendLine();
            sb.AppendLine("Return ONLY the JSON. No markdown. No explanation.");

            return sb.ToString();
        }
    }
}