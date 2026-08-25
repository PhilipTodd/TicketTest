---
layout: default
title: Architecture
permalink: /architecture/
---

# Architecture

The Ticketing Reference Application uses a deliberately straightforward architecture appropriate to the size and complexity of the application.

## Application Architecture

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

### Angular Client

The Angular single-page application provides the user interface for:

- Viewing and filtering tickets
- Creating tickets
- Editing tickets
- Deleting tickets
- Displaying validation and concurrency errors

Environment-specific configuration allows local development to use the locally hosted API while the production build calls the Azure-hosted API.

### ASP.NET Core API

The REST API owns the application's business rules and persistence operations.

The API provides endpoints for:

- Retrieving individual tickets
- Searching, filtering, sorting and paging tickets
- Creating tickets
- Updating tickets
- Deleting tickets

Validation includes ticket status transitions, priority rules, assignment requirements and optimistic concurrency checks.

### Data Access

Entity Framework Core provides direct access to the application's data through `AppDbContext`.

The deployed application uses **Azure SQL**, with application-owned objects isolated within the:

```text
ticketing
```

schema of the shared:

```text
ReferenceProjectsDb
```

database.

EF Core migrations manage the database schema, with migration history maintained independently for the Ticketing application.

## Azure Architecture

```text
                    Internet
                       |
          +------------+------------+
          |                         |
          v                         v
 Azure Static Web Apps        Azure App Service
     Angular SPA               ASP.NET Core API
                                      |
                                      | EF Core
                                      v
                                  Azure SQL
                              ReferenceProjectsDb
                               ticketing schema

                              Application Insights
                                      |
                                      v
                                Log Analytics
```

The application uses a combination of application-owned and shared platform resources.

### Application-Owned Resources

- Azure resource group
- Static Web App
- API App Service

### Shared Platform Resources

- Linux App Service Plan
- Azure SQL logical server
- `ReferenceProjectsDb`
- Application Insights
- Log Analytics

This allows several reference applications to share cost-effective Azure platform infrastructure while retaining separation of application-specific resources and database schemas.

## Infrastructure as Code

Azure infrastructure is defined using **Bicep**.

The subscription-level deployment creates the application's resource group and uses resource-group-scoped Bicep modules to deploy application resources.

Existing shared platform resources are referenced rather than recreated.

## CI/CD

Azure DevOps pipelines provide separate workflows for:

- Infrastructure validation and deployment
- API build, automated testing and deployment
- Angular build and Static Web App deployment

Infrastructure deployments use Bicep build, validation and What-If operations before deployment.