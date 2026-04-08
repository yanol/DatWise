using Dapper;
using NUnit.Framework;
using SafetyCompliance.Models;
using SafetyCompliance.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SafetyCompliance.Tests
{
    [TestFixture]
    public class TestComplianceRepository
    {
        private ComplianceRepository _repository;

        [SetUp]
        public async Task SetUp()
        {
            string masterConnString = @"Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=True;";

            string testDbName = "SafetyComplianceTest";
            string testConnString = $@"Server=(localdb)\mssqllocaldb;Database={testDbName};Trusted_Connection=True;";

            _repository = new ComplianceRepository(testConnString);

            using (IDbConnection masterConn = new SqlConnection(masterConnString))
            {
                await masterConn.ExecuteAsync($@"
                        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{testDbName}')
                        BEGIN
                            CREATE DATABASE [{testDbName}]
                        END");
            }

            using (IDbConnection testDbConn = new SqlConnection(testConnString))
            {
                const string createTableSql = @"
                            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditResults')
                            BEGIN
                                CREATE TABLE AuditResults (
                                    Id INT IDENTITY(1,1) PRIMARY KEY,
                                    OfficerId INT,
                                    AuditType NVARCHAR(50),
                                    Summary NVARCHAR(MAX),
                                    GapsJson NVARCHAR(MAX),
                                    ReadinessScore INT,
                                    CriticalCount INT,
                                    MediumCount INT,
                                    LowCount INT,
                                    ScanDate DATETIME
                                )
                            END";

                await testDbConn.ExecuteAsync(createTableSql);
            }
        }

        [Test]
        public void GetComplianceDataAsync_DatabaseUnavailable_ThrowsDbException()
        {
            var invalidRepo = new ComplianceRepository("Server=non_existent_server;Database=none;Connect Timeout=2;");

            Assert.That(
                async () => await invalidRepo.GetComplianceDataAsync("full", DateTime.Now, DateTime.Now),
                Throws.InstanceOf<Exception>());
        }

        [Test]
        [Category("Integration")]
        public async Task SaveAndRetrieve_FullCycle_DataIntegrityTest()
        {
            var testResult = new AuditResult
            {
                Summary = "Integration Test Summary",
                ReadinessScore = 85,
                Gaps = new List<ComplianceGap>
                {
                    new ComplianceGap { Severity = "critical", Finding = "Broken Railing", Domain = "Safety" },
                    new ComplianceGap { Severity = "critical", Finding = "Missing Sign", Domain = "Safety" },
                    new ComplianceGap { Severity = "medium", Finding = "Loose Tile", Domain = "Safety" }
                }
            };

            int newId = await _repository.SaveAuditResultAsync(testResult, 1, "full");

            var retrieved = await _repository.GetAuditResultByIdAsync(newId);

            Assert.That(retrieved, Is.Not.Null);
            Assert.That(retrieved.Summary, Is.EqualTo(testResult.Summary));

            Assert.That(retrieved.CriticalCount, Is.EqualTo(2), "CriticalCount should be calculated from Gaps list");
            Assert.That(retrieved.MediumCount, Is.EqualTo(1));
            Assert.That(retrieved.LowCount, Is.EqualTo(0));
        }
    }
}