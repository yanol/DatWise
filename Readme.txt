SafetyCompliance AI Auditor 🛡️🤖

SafetyCompliance AI Auditor is an intelligent AI agent designed to automate safety audits in industrial and high-risk environments. By wrapping legacy safety data in a modern AI layer, the system transforms fragmented SQL records into actionable compliance insights in seconds.

📌 Business Need

In high-risk sectors (especially in Israel, complying with Ministry of Labor regulations, ISO 45001, etc.), safety compliance is often managed through disconnected databases. 
Security officers traditionally spend days manually auditing training records, equipment inspections, and open incidents.
The Problem: Manual auditing is slow, reactive, and prone to human error.
The Solution: This agent automates the entire scan, providing a proactive "Readiness Score" and identifying gaps before they become hazards.

🚀 Solution Overview

The system acts as an intelligent bridge between raw legacy data and executive decision-making. 
By integrating the Groq AI (Llama 3) model, the system:
- Automates Analysis: Pulls raw safety data via Dapper and uses LLMs to identify specific risks.
- Scores Readiness: Calculates a real-time Readiness Score (0-100%) based on audit findings.
- Actionable Insights: Generates AI-driven recommendations on how to close compliance gaps.

The agent flow has two phases — a pre-audit scan, and then an interactive gap-fixing session (should be done in second phase):

## Audit Process Flow
![Audit Flow Diagram](audit-flow.jpg.png)
[cite_start]*תרשים המתאר את זרימת המידע מהשאילתה ב-SQL Server ועד לניתוח ה-AI והפקת הדוח[cite: 4].*

🏗️ Architectural Overview

The project follows a Layered Architecture to ensure high maintainability and testability:
- Presentation Layer: ASP.NET WebForms dashboard (Auditscan.aspx).
- Service Layer: Business logic (ComplianceAgentService) coordinating between database state and AI analysis.
- Infrastructure Layer: Handles external Groq API calls and efficient data access using ComplianceRepository.
- AI Integration: A specialized PromptBuilder transforms raw SQL rows into natural language context for the LLM.

🛠️ Technical Stack

- Backend: .NET / C#
- Data Access: Dapper (Micro-ORM) for high-performance SQL queries.
- Database: SQL Server
- AI Engine: Groq API (Llama 3)
- JSON Handling: Newtonsoft.Json for complex data serialization.

📊 Repository Features (ComplianceRepository.cs)

The repository provides comprehensive coverage for safety audits:

- Training Gaps: Identifies expired certificates or those expiring within a 30-day window.
- Equipment Gaps: Tracks overdue inspections based on required frequencies.
- Open Incidents: Filters and summarizes incidents within specific date ranges.
- Permit Monitoring: Alerts on expiring work permits (60-day window).
- Emergency Drills: Validates compliance with drill frequency regulations.

⚙️ How to Run Locally

- Database: Run the provided schema.sql on your SQL Server instance.
- Configuration: Update the SafetyDB connection string in Web.config.
- API Key: Add your GroqApiKey to the appSettings section.
- Tests: Run the NUnit 3 test suite to verify the Integration Heat Tests and Data Integrity.

🚀 Future Roadmap: Phase 2

The next iteration of the SafetyCompliance AI Auditor will focus on enterprise security and enhanced user experience:

1. Enterprise Authentication (Okta Integration)
Secure Access: Add authentication page. Add authentication with Okta OIDC (OpenID Connect). 
Role-Based Access Control (RBAC): Restrict audit capabilities based on user groups (e.g., Safety Officer vs. View-only Auditor).

2. Polling
Polling Logic: Implementation of a frontend polling mechanism (or SignalR) to track the status of the "Audit in Progress" and notify the user once the report is ready.

3. Resilience & Connectivity
Offline Handling: Graceful handling of internet disconnections during API calls to Groq/Claude.
Retry Policy: Implementation of Polly (a .NET resilience library) to handle transient network errors and API rate limits automatically.






