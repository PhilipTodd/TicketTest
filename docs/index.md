---
layout: default
title: Ticketing Reference Application
permalink: /
---

# Ticketing Reference Application

A small full-stack reference application demonstrating the design, implementation, testing and Azure deployment of a ticket management system.

The project originated as a **Senior C# Developer technical assessment** and was subsequently extended into a publicly hosted reference application.

## Overview

The application provides a simple ticket management workflow supporting:

- Create, view, update and delete tickets
- Filtering, searching, sorting and pagination
- Ticket status and priority business rules
- Optimistic concurrency handling
- Validation and structured API error responses
- Automated integration testing

The implementation deliberately keeps the application architecture proportionate to the size and complexity of the problem, while demonstrating production-oriented engineering practices around testing, cloud infrastructure and deployment.

## Technology

### Application

- **ASP.NET Core** REST API
- **Angular** single-page application
- **Entity Framework Core**
- **Azure SQL**
- **xUnit** integration tests

### Azure

- **Azure App Service** — API hosting
- **Azure Static Web Apps** — Angular application hosting
- **Azure SQL Database** — persistent storage
- **Application Insights** — application telemetry
- **Log Analytics** — centralised logging and diagnostics

Shared Azure platform resources are reused where appropriate, while application-specific resources are maintained independently.

### Delivery

- **Bicep** — infrastructure as code
- **Azure DevOps Pipelines** — build, test and deployment automation
- **GitHub** — public source repository
- **GitHub Pages / Jekyll** — project documentation

## Architecture

The application intentionally uses a straightforward architecture:

```text
Angular SPA
    |
    | HTTPS / REST
    v
ASP.NET Core API
    |
    | Entity Framework Core
    v
Azure SQL
ReferenceProjectsDb
ticketing schema
```

Patterns such as repositories, MediatR, CQRS and additional application/domain-service layers were deliberately not introduced. For the current scope, these abstractions would add structural complexity and indirection without solving corresponding application complexity.

The reasoning behind these decisions is discussed in more detail in [Engineering Decisions](engineering-decisions).

## Concurrency

Ticket updates and deletes use a version-based optimistic concurrency mechanism. Clients submit the version originally retrieved with the ticket, and the API rejects operations when that version no longer matches the current ticket.

The implementation provides useful stale-client detection but has known limitations compared with database-enforced optimistic concurrency using SQL Server `rowversion`. These trade-offs are documented in [Engineering Decisions](engineering-decisions).

## Testing

The API is covered by automated integration tests that exercise HTTP endpoints and application behaviour.

Production uses Azure SQL, while integration tests use an isolated in-memory SQLite database. This keeps the test suite fast and independent of external Azure infrastructure.

## AI-Assisted Development

AI tools were used during development for implementation suggestions, troubleshooting, test scenarios, SQL review, infrastructure design and documentation.

AI-generated output was treated as proposed engineering input rather than authoritative output. Changes were reviewed, compiled, tested and validated before being incorporated into the project.

See [AI-Assisted Development](ai-assisted-development) for further details.

---

## Documentation

- [Architecture](/architecture/)
- [Engineering Decisions](/engineering-decisions/)
- [Azure Hosting and Delivery](/azure/)
- [AI-Assisted Development](/ai-assisted-development/)

---

## Source Code

[GitHub Repository](https://github.com/philiptodd/TicketTest){:target="_blank"}

---

## Live Demo

[Launch Ticketing Demo](https://demo.ticketing.ausdatatech.com.au/){:target="_blank"}