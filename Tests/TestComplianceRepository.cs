using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Common;
using SafetyCompliance.Repositories;
using SafetyCompliance.Models;

namespace SafetyCompliance.Tests
{
    [TestFixture]
    public class ComplianceRepositoryTests
    {
        private const string TestConnectionString = "Server=(localdb)\\mssqllocaldb;Database=SafetyComplianceTest;Trusted_Connection=True;";
        private ComplianceRepository _repository;

        [SetUp]
        public void SetUp()
        {
            _repository = new ComplianceRepository(TestConnectionString);
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