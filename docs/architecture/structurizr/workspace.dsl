workspace "Ticketing Reference Application" "C4 architecture model for the Ticketing Reference Application." {

    model {

        // ====================================================================
        // People
        // ====================================================================

        user = person "User" "Uses the application to view, create, edit and delete support tickets."

        // ====================================================================
        // Ticketing System
        // ====================================================================

        ticketing = softwareSystem "Ticketing Reference Application" "A full-stack ticket management reference application built with Angular, ASP.NET Core and Azure SQL." {

            web = container "Ticketing Web UI" "Provides the browser-based ticket management interface, including filtering, paging, creation, editing and deletion." "Angular"

            api = container "Ticketing API" "Provides REST endpoints for ticket queries and commands, validates business rules and manages persistence." "ASP.NET Core / .NET 8"

            database = container "Ticketing Database" "Stores ticket state and EF Core migration history within the ticketing schema of ReferenceProjectsDb." "Azure SQL / Entity Framework Core"

        }

        // ====================================================================
        // External Systems
        // ====================================================================

        appInsights = softwareSystem "Application Insights" "Collects application telemetry, requests, dependencies and exceptions." "Azure Application Insights"

        logAnalytics = softwareSystem "Log Analytics" "Provides centralised storage and querying of Azure operational telemetry." "Azure Log Analytics"

        github = softwareSystem "GitHub" "Hosts the public application source code and project documentation." "GitHub"

        azureDevOps = softwareSystem "Azure DevOps" "Builds, tests and deploys the application and its Azure infrastructure." "Azure DevOps Pipelines"

        // ====================================================================
        // Relationships
        // ====================================================================

        user -> web "Uses" "HTTPS"

        web -> api "Calls REST API" "HTTPS / JSON"

        api -> database "Reads and writes tickets" "Entity Framework Core / TDS"

        api -> appInsights "Sends application telemetry" "Application Insights SDK"

        appInsights -> logAnalytics "Stores and exposes telemetry" "Azure Monitor"

        github -> azureDevOps "Provides source code" "Git"

        azureDevOps -> web "Builds and deploys" "Azure Static Web Apps deployment"

        azureDevOps -> api "Builds, tests and deploys" "Azure App Service deployment"

    }

    views {

        // ====================================================================
        // System Context
        // ====================================================================

        systemContext ticketing "SystemContext" {

            include user
            include ticketing
            include appInsights
            include github
            include azureDevOps

            autolayout lr

            title "Ticketing Reference Application - System Context"
            description "Shows the Ticketing application, its users and supporting development and operational systems."
        }

        // ====================================================================
        // Container View
        // ====================================================================

        container ticketing "Containers" {

            include user

            include web
            include api
            include database

            include appInsights
            include logAnalytics

            autolayout lr

            title "Ticketing Reference Application - Containers"
            description "Shows the Angular web application, ASP.NET Core API, Azure SQL persistence and observability services."
        }


        // ====================================================================
        // Styles
        // ====================================================================

        styles {

          element "Person" {
            shape person
          }

          element "External" {
            border dashed
          }

          element "System" {
            shape roundedBox
          }

          element "Web" {
            shape roundedBox
          }

          element "Gateway" {
            shape roundedBox
          }

          element "Service" {
            shape roundedBox
          }

          element "Database" {
            shape cylinder
          }

          element "Messaging" {
            shape pipe
          }

          element "Infra" {
            shape roundedBox
          }
        }


    }

}