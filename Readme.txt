SafetyCompliance AI Auditor

1. Business Need

In many industrial and high-risk environments, safety compliance is managed through fragmented SQL databases. 

A security officer in Israel must comply with multiple regulatory bodies (משרד העבודה, תקנות הבטיחות בעבודה, ISO 45001, etc.). 

Before every audit, they spend days manually checking: are all training certificates valid? Are all incidents properly documented and closed? 
Are inspections up to date? 

This agent does that scan in seconds and tells them exactly what's missing.

Manual auditing is slow, prone to human error, and often reactive.

2. Solution Overview

The SafetyCompliance AI Auditor is an intelligent wrapper around legacy safety data. By integrating the Groq AI (Llama 3) model, the system:


Automates Analysis: Pulls raw data from SQL and uses LLMs to identify risks.

Scores Readiness: Provides a real-time "Readiness Score" (0-100%) based on the severity of findings.

Generates Recommendations: Moves beyond "what is wrong" to "how to fix it" with AI-generated action items.

3. Architectural Overview

The project follows a Layered Architecture with Dependency Injection (DI) to ensure maintainability and testability.

Presentation Layer: ASP.NET WebForms (Auditscan.aspx) providing a real-time dashboard.

Service Layer: Contains the business logic (ComplianceAgentService) and coordinates between the DB and AI.

Infrastructure Layer: Handles external integrations (GroqApiClient) and Data Access (ComplianceRepository).

AI Integration: Uses a PromptBuilder to transform SQL rows into natural language for the LLM.

4. Key Components