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

🏗️ Architectural Overview

The project follows a Layered Architecture to ensure high maintainability and testability:
- Presentation Layer: ASP.NET WebForms dashboard (Auditscan.aspx).
- Service Layer: Business logic (ComplianceAgentService) coordinating between database state and AI analysis.
- Infrastructure Layer: Handles external Groq API calls and efficient data access using ComplianceRepository.
- AI Integration: A specialized PromptBuilder transforms raw SQL rows into natural language context for the LLM.
