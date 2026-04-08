using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SafetyCompliance.Models
{
    // ── Single finding returned by AI ──
    public class ComplianceGap
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("finding")]
        public string Finding { get; set; }

        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("deadline")]
        public string Deadline { get; set; }

        [JsonProperty("recommendation")]
        public string Recommendation { get; set; }

        public string SeverityLabel
        {
            get
            {
                return (Severity ?? "").ToLower() switch
                {
                    "critical" => "Critical",
                    "medium" => "Medium",
                    "low" => "Low",
                    _ => Severity ?? ""
                };
            }
        }
    }

    // ── Raw data pulled from SQL before sending to AI ─
    public class ComplianceData
    {
        public List<TrainingGap> TrainingGaps { get; set; } = new List<TrainingGap>();
        public List<OpenIncident> OpenIncidents { get; set; } = new List<OpenIncident>();
        public List<EquipmentGap> EquipmentGaps { get; set; } = new List<EquipmentGap>();
        public List<PermitGap> PermitGaps { get; set; } = new List<PermitGap>();
        public List<DrillGap> DrillGaps { get; set; } = new List<DrillGap>();
        public string AuditType { get; set; }
        public bool IsEmpty =>
            TrainingGaps.Count == 0 &&
            OpenIncidents.Count == 0 &&
            EquipmentGaps.Count == 0 &&
            PermitGaps.Count == 0 &&
            DrillGaps.Count == 0;
    }

    public class TrainingGap
    {
        public string Employee { get; set; }
        public string CourseName { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class OpenIncident
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }

    public class EquipmentGap
    {
        public string Equipment { get; set; }
        public DateTime LastInspection { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class PermitGap
    {
        public string PermitType { get; set; }
        public string HolderName { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class DrillGap
    {
        public string DrillType { get; set; }
        public DateTime LastDrillDate { get; set; }
        public int MonthsOverdue { get; set; }
    }

    // ── Final structured result from AI ─
    public class AuditResult
    {
        public int AuditId { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("gaps")]
        public List<ComplianceGap> Gaps { get; set; } = new List<ComplianceGap>();

        [JsonProperty("readinessScore")]
        public int ReadinessScore { get; set; }

        public DateTime ScanDate { get; set; } = DateTime.Now;

        public string GapsJson { get; set; }

        public int CriticalCount => Gaps.FindAll(g =>
                        string.Equals(g.Severity, "critical", StringComparison.OrdinalIgnoreCase)).Count;

        public int MediumCount => Gaps.FindAll(g =>
                        string.Equals(g.Severity, "medium", StringComparison.OrdinalIgnoreCase)).Count;

        public int LowCount => Gaps.FindAll(g =>
                        string.Equals(g.Severity, "low", StringComparison.OrdinalIgnoreCase)).Count;
    }
}
