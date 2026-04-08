using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using SafetyCompliance.Models;

namespace SafetyCompliance.Repositories
{
    public class ComplianceRepository : IComplianceRepository
    {
        private readonly string _connString;

        public ComplianceRepository()
        {
            _connString = ConfigurationManager.ConnectionStrings["SafetyDB"].ConnectionString;
        }

        public ComplianceRepository(string connString)
        {
            _connString = connString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connString);

        public async Task<ComplianceData> GetComplianceDataAsync(string auditType, DateTime dateFrom, DateTime dateTo)
        {
            var data = new ComplianceData { AuditType = auditType };
            bool isFull = auditType == "full";

            using (var db = CreateConnection())
            {
                if (isFull || auditType == "training")
                    data.TrainingGaps = (await GetExpiredTrainingAsync(db, dateFrom, dateTo)).ToList();

                if (isFull || auditType == "incidents")
                    data.OpenIncidents = (await GetOpenIncidentsAsync(db, dateFrom, dateTo)).ToList();

                if (isFull || auditType == "equipment")
                    data.EquipmentGaps = (await GetOverdueEquipmentAsync(db)).ToList();

                if (isFull || auditType == "permits")
                    data.PermitGaps = (await GetExpiringPermitsAsync(db)).ToList();

                if (isFull)
                    data.DrillGaps = (await GetOverdueDrillsAsync(db)).ToList();
            }

            return data;
        }

        private async Task<IEnumerable<TrainingGap>> GetExpiredTrainingAsync(IDbConnection db, DateTime from, DateTime to)
        {
            const string sql = @"
                SELECT e.FullName  AS EmployeeName,
                       t.CourseName,
                       t.ExpiryDate
                FROM   TrainingRecords t
                JOIN   Employees       e ON e.Id = t.EmployeeId
                WHERE  t.ExpiryDate <= DATEADD(DAY, 30, GETDATE())
                  AND  t.IsActive = 1
                ORDER  BY t.ExpiryDate ASC";

            return await db.QueryAsync<TrainingGap>(sql, new { from, to });
        }

        private async Task<IEnumerable<OpenIncident>> GetOpenIncidentsAsync(IDbConnection db, DateTime from, DateTime to)
        {
            const string sql = @"
                SELECT IncidentDate as Date, Description, Location 
                FROM Incidents 
                WHERE IncidentDate BETWEEN @from AND @to";

            return await db.QueryAsync<OpenIncident>(sql, new { from, to });
        }

        private async Task<IEnumerable<EquipmentGap>> GetOverdueEquipmentAsync(IDbConnection db)
        {
            const string sql = @"
                SELECT EquipmentName as Equipment, LastInspectionDate as LastInspection, 
                       DATEDIFF(day, NextInspectionDue, GETDATE()) as DaysOverdue
                FROM Equipment 
                WHERE NextInspectionDue < GETDATE()";

            return await db.QueryAsync<EquipmentGap>(sql);
        }

        private async Task<IEnumerable<PermitGap>> GetExpiringPermitsAsync(IDbConnection db)
        {
            const string sql = @"
                SELECT p.PermitType,
                       e.FullName  AS HolderName,
                       p.ExpiryDate
                FROM   Permits   p
                JOIN   Employees e ON e.Id = p.EmployeeId
                WHERE  p.ExpiryDate < DATEADD(DAY, 60, GETDATE())
                  AND  p.IsActive = 1";

            return await db.QueryAsync<PermitGap>(sql);
        }

        private async Task<IEnumerable<DrillGap>> GetOverdueDrillsAsync(IDbConnection db)
        {
            const string sql = @"
                SELECT DrillType,
                       DrillDate  AS LastDrillDate,
                       DATEDIFF(MONTH, DrillDate, GETDATE()) AS MonthsOverdue
                FROM   EmergencyDrills
                WHERE  DATEDIFF(MONTH, DrillDate, GETDATE()) > RequiredFrequencyMonths";

            return await db.QueryAsync<DrillGap>(sql);
        }

        public async Task<int> SaveAuditResultAsync(AuditResult result, int officerId, string auditType)
        {
            const string sql = @"
                INSERT INTO AuditResults (
                    OfficerId, AuditType, Summary, GapsJson, 
                    ReadinessScore, CriticalCount, MediumCount, LowCount, ScanDate
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @OfficerId, @AuditType, @Summary, @GapsJson, 
                    @ReadinessScore, @CriticalCount, @MediumCount, @LowCount, GETDATE()
                );";

            using (var db = CreateConnection())
            {
                var parameters = new
                {
                    OfficerId = officerId,
                    AuditType = auditType,
                    Summary = result.Summary ?? "No summary provided",
                    GapsJson = JsonConvert.SerializeObject(result.Gaps ?? new List<ComplianceGap>()),
                    ReadinessScore = result.ReadinessScore,
                    CriticalCount = result.CriticalCount,
                    MediumCount = result.MediumCount,
                    LowCount = result.LowCount
                };

                return await db.ExecuteScalarAsync<int>(sql, parameters);
            }
        }

        public async Task<AuditResult> GetAuditResultByIdAsync(int auditId)
        {
            using (var db = CreateConnection())
            {
                const string sql = "SELECT * FROM AuditResults WHERE Id = @auditId";

                var result = await db.QueryFirstOrDefaultAsync<AuditResult>(sql, new { auditId });

                if (result != null && !string.IsNullOrEmpty(result.GapsJson))
                {
                    result.Gaps = JsonConvert.DeserializeObject<List<ComplianceGap>>(result.GapsJson);
                }
                return result;
            }
        }
    }
}