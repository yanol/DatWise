using SafetyCompliance.Helpers;
using SafetyCompliance.Models;
using SafetyCompliance.Repositories;
using System;
using System.Threading.Tasks;
using System.Web.UI;

namespace SafetyCompliance.Services
{
    public class ComplianceAgentService : IComplianceAgentService
    {
        private readonly IComplianceRepository _repo;
        private readonly IAiClient _aiClient;
        private readonly ISystemLogger _logger;

        public ComplianceAgentService(IComplianceRepository repo, IAiClient aiClient, ISystemLogger logger)
        {
            _repo = repo;
            _aiClient = aiClient;
            _logger = logger;
        }

        public async Task<AuditResult> RunScanAsync(
     string auditType,
     DateTime dateFrom,
     DateTime dateTo,
     int officerId)
        {
            try
            {
                await _logger.LogAsync(officerId, "Running AI Analysis");

                var data = await _repo.GetComplianceDataAsync(auditType, dateFrom, dateTo);

                if (data == null || data.IsEmpty)
                    return new AuditResult
                    {
                        ReadinessScore = 100,
                        Summary = "No findings"
                    };

                var result = await _aiClient.AnalyzeComplianceAsync(data);

                result.ScanDate = DateTime.Now;
                result.AuditId = await _repo.SaveAuditResultAsync(result, officerId, auditType);

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(officerId, "AI Analysis Failed", ex.Message);

                throw new Exception("Compliance scan failed due to an internal error.", ex);
            }
        }
    }
}
