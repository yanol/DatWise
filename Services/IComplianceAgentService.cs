using System;
using System.Threading.Tasks;
using SafetyCompliance.Models;

namespace SafetyCompliance.Services
{
    public interface IComplianceAgentService
    {
        Task<AuditResult> RunScanAsync(string auditType, DateTime dateFrom, DateTime dateTo, int officerId);
    }
}