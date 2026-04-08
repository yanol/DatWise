using Moq;
using NUnit.Framework;
using SafetyCompliance.Helpers;
using SafetyCompliance.Models;
using SafetyCompliance.Repositories;
using SafetyCompliance.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace SafetyCompliance.Tests
{
    [TestFixture]
    public class TestsComplianceAgentService
    {
        private Mock<IComplianceRepository> _repoMock;
        private Mock<IAiClient> _aiClientMock;
        private Mock<ISystemLogger> _loggerMock;
        private ComplianceAgentService _service;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IComplianceRepository>();
            _aiClientMock = new Mock<IAiClient>();
            _loggerMock = new Mock<ISystemLogger>();

            _service = new ComplianceAgentService(
                _repoMock.Object,
                _aiClientMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task RunScanAsync_WhenNoDataFound_ReturnsPerfectScoreWithoutAI()
        {
            _repoMock.Setup(r => r.GetComplianceDataAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new ComplianceData()); 

            // Act
            var result = await _service.RunScanAsync("full", DateTime.Now.AddDays(-7), DateTime.Now, 1);

            // Assert
            Assert.That(result.ReadinessScore, Is.EqualTo(100));
            Assert.That(result.Summary, Does.Contain("No findings"));

            _aiClientMock.Verify(a => a.AnalyzeComplianceAsync(It.IsAny<ComplianceData>()), Times.Never);
        }

        [Test]
        public async Task RunScanAsync_WhenDataExists_CallsAIAndSavesResult()
        {
            // Arrange
            var auditData = new ComplianceData
            {
                TrainingGaps = new List<TrainingGap> { new TrainingGap { Employee = "Test" } }
            };
            var aiResult = new AuditResult { ReadinessScore = 75, Summary = "Issues found" };

            _repoMock.Setup(r => r.GetComplianceDataAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                     .ReturnsAsync(auditData);

            _aiClientMock.Setup(a => a.AnalyzeComplianceAsync(auditData))
                         .ReturnsAsync(aiResult);

            _repoMock.Setup(r => r.SaveAuditResultAsync(aiResult, It.IsAny<int>(), It.IsAny<string>()))
                     .ReturnsAsync(101);

            // Act
            var result = await _service.RunScanAsync("full", DateTime.Now, DateTime.Now, 1);

            // Assert
            Assert.That(result.AuditId, Is.EqualTo(101));
            _repoMock.Verify(r => r.SaveAuditResultAsync(It.IsAny<AuditResult>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _loggerMock.Verify(l => l.LogAsync(It.IsAny<int>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public async Task RunScanAsync_WhenAiFails_ShouldLogErrorAndThrowFriendlyException()
        {
            _repoMock.Setup(r => r.GetComplianceDataAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                     .ReturnsAsync(new ComplianceData
                     {
                         TrainingGaps = new List<TrainingGap> { new TrainingGap() }
                     });

            _aiClientMock.Setup(a => a.AnalyzeComplianceAsync(It.IsAny<ComplianceData>()))
                         .ThrowsAsync(new Exception("API Down"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _service.RunScanAsync("full", DateTime.Now, DateTime.Now, 1));

            Assert.That(ex.Message, Is.EqualTo("Compliance scan failed due to an internal error."));

            _loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}