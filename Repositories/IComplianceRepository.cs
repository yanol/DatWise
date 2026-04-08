using SafetyCompliance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SafetyCompliance.Repositories
{
    public interface IComplianceRepository
    {
        Task<ComplianceData> GetComplianceDataAsync(string auditType, DateTime dateFrom, DateTime dateTo);
        Task<int> SaveAuditResultAsync(AuditResult result, int officerId, string auditType);
        Task<AuditResult> GetAuditResultByIdAsync(int auditId);
    }
}