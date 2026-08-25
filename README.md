# TicketTest

Reference ticket management application built with **ASP.NET Core**,
**Angular**, **Entity Framework Core**, and **Azure SQL**.

## Prerequisites

Install:

-   .NET 8 SDK
-   Node.js 22+
-   Angular CLI
-   Access to the development Azure SQL database, or configure an
    appropriate local SQL Server database

## Run the API

From the repository root:

``` powershell
cd TicketTest.Api
```

Configure the database connection string using .NET User Secrets:

``` powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>"
```

Apply any outstanding EF Core migrations:

``` powershell
dotnet ef database update
```

Start the API:

``` powershell
dotnet run
```

The API URL is displayed in the console when the application starts.
Swagger is available at:

``` text
https://localhost:<port>/swagger
```

## Run the Angular Client

Open another terminal:

``` powershell
cd TicketTest.Client
npm install
npm start
```

Browse to:

``` text
http://localhost:4200
```

The development Angular environment is configured to call the locally
running API.

## Run Tests

From the repository root:

``` powershell
dotnet test
```

Integration tests use an isolated in-memory SQLite database and do not
modify the Azure SQL database.

## Production

The hosted application uses:

-   Azure Static Web Apps --- Angular client
-   Azure App Service --- ASP.NET Core API
-   Azure SQL --- persistence
-   Application Insights / Log Analytics --- monitoring
-   Azure DevOps --- CI/CD
-   Bicep --- infrastructure as code
