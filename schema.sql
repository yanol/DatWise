-- ============================================================
-- SafetyCompliance Database Schema
-- Run this script once on your SQL Server instance
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SafetyCompliance')
    CREATE DATABASE SafetyCompliance;
GO

USE SafetyCompliance;
GO

-- ── Employees ────────────────────────────────────────────────────────────
CREATE TABLE Employees (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    FullName    NVARCHAR(100) NOT NULL,
    Department  NVARCHAR(80),
    Role        NVARCHAR(80),
    IsActive    BIT           NOT NULL DEFAULT 1,
    CreatedDate DATETIME      NOT NULL DEFAULT GETDATE()
);

-- ── Training Records ─────────────────────────────────────────────────────
CREATE TABLE TrainingRecords (
    Id            INT           IDENTITY(1,1) PRIMARY KEY,
    EmployeeId    INT           NOT NULL REFERENCES Employees(Id),
    CourseName    NVARCHAR(120) NOT NULL,
    CompletedDate DATETIME      NOT NULL,
    ExpiryDate    DATETIME      NOT NULL,
    IsActive      BIT           NOT NULL DEFAULT 1
);

-- ── Incidents ────────────────────────────────────────────────────────────
CREATE TABLE Incidents (
    Id                 INT           IDENTITY(1,1) PRIMARY KEY,
    IncidentDate       DATETIME      NOT NULL,
    Description        NVARCHAR(500) NOT NULL,
    Location           NVARCHAR(120),
    Severity           NVARCHAR(20),  -- critical | medium | low
    CorrectiveActionId INT           NULL,  -- NULL = no action assigned yet
    ReportedBy         INT           REFERENCES Employees(Id),
    CreatedDate        DATETIME      NOT NULL DEFAULT GETDATE()
);

-- ── Equipment ────────────────────────────────────────────────────────────
CREATE TABLE Equipment (
    Id                    INT           IDENTITY(1,1) PRIMARY KEY,
    EquipmentName         NVARCHAR(120) NOT NULL,
    LastInspectionDate    DATETIME,
    NextInspectionDue     DATETIME,
    RequiredFrequencyDays INT           NOT NULL DEFAULT 365,
    IsActive              BIT           NOT NULL DEFAULT 1
);

-- ── Permits ──────────────────────────────────────────────────────────────
CREATE TABLE Permits (
    Id         INT           IDENTITY(1,1) PRIMARY KEY,
    PermitType NVARCHAR(100) NOT NULL,
    EmployeeId INT           NOT NULL REFERENCES Employees(Id),
    IssuedDate DATETIME      NOT NULL,
    ExpiryDate DATETIME      NOT NULL,
    IsActive   BIT           NOT NULL DEFAULT 1
);

-- ── Emergency Drills ─────────────────────────────────────────────────────
CREATE TABLE EmergencyDrills (
    Id                      INT           IDENTITY(1,1) PRIMARY KEY,
    DrillType               NVARCHAR(100) NOT NULL,
    DrillDate               DATETIME      NOT NULL,
    Participants            INT,
    RequiredFrequencyMonths INT           NOT NULL DEFAULT 6,
    Notes                   NVARCHAR(300)
);

-- ── Audit Results (saved after each AI scan) ─────────────────────────────
CREATE TABLE AuditResults (
    Id             INT            IDENTITY(1,1) PRIMARY KEY,
    OfficerId      INT            REFERENCES Employees(Id),
    AuditType      NVARCHAR(50)   NOT NULL,
    ScanDate       DATETIME       NOT NULL DEFAULT GETDATE(),
    ReadinessScore INT            NOT NULL,
    Summary        NVARCHAR(1000),
    GapsJson       NVARCHAR(MAX),  -- full JSON of ComplianceGap list
    CriticalCount  INT            NOT NULL DEFAULT 0,
    MediumCount    INT            NOT NULL DEFAULT 0,
    LowCount       INT            NOT NULL DEFAULT 0
);

-- ── System Logs ──────────────────────────────────────────────────────────
CREATE TABLE SystemLogs (
    Id       INT           IDENTITY(1,1) PRIMARY KEY,
    UserId   INT,
    Action   NVARCHAR(100) NOT NULL,
    Details  NVARCHAR(MAX),
    LogDate  DATETIME      NOT NULL DEFAULT GETDATE(),
    LogLevel NVARCHAR(10)  NOT NULL DEFAULT 'INFO'  -- INFO | ERROR
);
GO

-- ── Sample data for testing ──────────────────────────────────────────────
INSERT INTO Employees (FullName, Department, Role) VALUES
    ('David Levy',    'Production',  'Worker'),
    ('Sarah Cohen',   'Logistics',   'Driver'),
    ('Michael Avi',   'Maintenance', 'Technician'),
    ('Ruth Mizrahi',  'Warehouse',   'Worker'),
    ('Chief Officer', 'Safety',      'Safety Officer');

INSERT INTO TrainingRecords (EmployeeId, CourseName, CompletedDate, ExpiryDate) VALUES
    (1, 'First Aid',              '2024-01-15', '2025-01-15'),  -- expired
    (2, 'Fire Safety',            '2024-06-01', '2026-06-01'),  -- ok
    (3, 'Working at Heights',     '2023-12-01', '2025-12-01'),  -- expiring soon
    (4, 'Hazardous Materials',    '2024-03-01', '2026-03-01');  -- ok

INSERT INTO Incidents (IncidentDate, Description, Location, Severity) VALUES
    ('2025-11-10', 'Slip near production line',          'Production Hall A', 'medium'),
    ('2025-12-05', 'Protective equipment not worn correctly', 'Warehouse',    'critical');

INSERT INTO Equipment (EquipmentName, LastInspectionDate, NextInspectionDue) VALUES
    ('Main Lifting Crane',   '2024-06-01', '2025-06-01'),  -- overdue
    ('Emergency Generator',  '2025-01-15', '2026-01-15'),  -- ok
    ('Fire Suppression System', '2024-03-01', '2025-03-01');  -- overdue

INSERT INTO Permits (PermitType, EmployeeId, IssuedDate, ExpiryDate) VALUES
    ('Forklift Driving License', 2, '2023-01-01', '2025-02-01'),  -- expired
    ('Working at Heights Permit', 3, '2024-01-01', '2026-01-01'); -- ok

INSERT INTO EmergencyDrills (DrillType, DrillDate, RequiredFrequencyMonths) VALUES
    ('Fire Evacuation Drill', '2024-06-01', 6),   -- overdue
    ('Earthquake Drill',      '2025-10-01', 12);  -- ok
GO
