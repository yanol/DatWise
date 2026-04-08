using SafetyCompliance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafetyCompliance.Services
{
    public interface IAiClient
    {
        Task<AuditResult> AnalyzeComplianceAsync(ComplianceData data);
    }
}
