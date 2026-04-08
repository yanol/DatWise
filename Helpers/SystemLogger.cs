using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;

namespace SafetyCompliance.Helpers
{
    public class SystemLogger : ISystemLogger
    {
        private readonly string _connString;

        public SystemLogger()
        {
            _connString = ConfigurationManager.ConnectionStrings["SafetyDB"].ConnectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connString);

        public async Task LogAsync(int userId, string action, string details = null)
        {
            await WriteAsync(userId, action, details, "INFO");
        }

        public async Task LogErrorAsync(int userId, string action, string errorMessage)
        {
            await WriteAsync(userId, action, errorMessage, "ERROR");
        }

        private async Task WriteAsync(int userId, string action, string details, string logLevel)
        {
            const string sql = @"
                INSERT INTO SystemLogs (UserId, Action, Details, LogDate, LogLevel)
                VALUES (@UserId, @Action, @Details, GETDATE(), @LogLevel)";

            try
            {
                using (var db = CreateConnection())
                {
                    await db.ExecuteAsync(sql, new
                    {
                        UserId = userId,
                        Action = action ?? string.Empty,
                        Details = details ?? string.Empty,
                        LogLevel = logLevel
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log to DB: {ex.Message}");
            }
        }
    }
}