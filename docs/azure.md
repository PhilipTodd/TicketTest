---
layout: default
title: Azure Hosting and Delivery
---

# Azure Hosting and Delivery

The original assessment application was extended into a publicly hosted Azure reference application.

Infrastructure is defined using Bicep and application delivery is automated through Azure DevOps.

## Hosting

### Angular Client

The Angular application is hosted using **Azure Static Web Apps**.

The public demo is available at:

[demo.ticketing.ausdatatech.com.au](https://demo.ticketing.ausdatatech.com.au)

The production Angular build is configured to communicate with the Azure-hosted API rather than the local development endpoint.

### ASP.NET Core API

The API is hosted as a Linux **Azure App Service**.

It runs on a shared Linux App Service Plan used by the reference-project platform.

HTTPS is enforced and application configuration is supplied through App Service environment settings.

### Azure SQL

Production persistence uses **Azure SQL**.

Rather than provisioning a separate database for each small reference application, the application uses the shared:

```text
ReferenceProjectsDb
```

database.

Application objects are isolated using the:

```text
ticketing
```

schema.

This includes the EF Core migration history, allowing migration ownership to remain separate from other applications sharing the database.

## Shared Platform Resources

The application reuses platform resources where appropriate:

- Linux App Service Plan
- Azure SQL logical server
- `ReferenceProjectsDb`
- Application Insights
- Log Analytics

Application-specific compute and web-hosting resources remain within the Ticketing resource group.

This approach reduces the operating cost of maintaining several publicly available reference applications.

## Infrastructure as Code

Infrastructure is defined using **Bicep** and stored alongside the application source.

The deployment operates at subscription scope so that it can create the application's resource group.

Resource-group-scoped modules then provision resources such as:

- API App Service
- Static Web App

Existing platform resources are declared as `existing` Bicep resources and referenced by the application deployment.

## Configuration and Secrets

Environment-specific settings are supplied through Azure configuration rather than embedded in application source code.

These include:

- Database connection configuration
- Application Insights configuration
- CORS origins
- Environment settings

Sensitive values are stored as secured Azure DevOps pipeline variables and supplied to infrastructure deployments without being committed to Git.

## CORS

The API explicitly allows the browser origins required by the application.

Development supports:

```text
http://localhost:4200
```

The hosted environment supports both:

```text
https://demo.ticketing.ausdatatech.com.au
```

and the default Azure Static Web App hostname.

The default hostname is obtained from the Static Web App Bicep deployment rather than being duplicated as manually maintained configuration.

## Database Migrations

Entity Framework Core migrations manage the Azure SQL schema.

Production schema creation does not use `EnsureCreated()`.

Integration tests continue to use `EnsureCreated()` because each test environment creates a disposable in-memory SQLite database.

This provides separate database lifecycle strategies appropriate to their respective environments.

## Azure DevOps

Separate Azure DevOps YAML pipelines are used for infrastructure, API and client delivery.

### API Pipeline

The API pipeline:

1. Restores dependencies.
2. Builds the .NET solution.
3. Runs automated integration tests.
4. Publishes test results.
5. Packages the API.
6. Deploys the application to Azure App Service.

### Client Pipeline

The client pipeline:

1. Installs Node.js dependencies using `npm ci`.
2. Builds the Angular production application.
3. Deploys the generated application to Azure Static Web Apps.

### Infrastructure Pipeline

The infrastructure pipeline performs:

```text
Bicep Build
     |
     v
Validation
     |
     v
What-If
     |
     v
Deployment
```

This provides an opportunity to detect syntax, configuration and unintended infrastructure changes before deployment.

## Observability

The API integrates with the shared **Application Insights** resource.

Application telemetry and diagnostics are available through Application Insights and the associated **Log Analytics** workspace.