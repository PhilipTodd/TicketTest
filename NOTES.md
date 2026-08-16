# Implementation Notes

## Running the Application

### Prerequisites

-   .NET 8 SDK
-   Node.js
-   Angular CLI

### API

From the repository root:

``` powershell
dotnet restore
dotnet run --project TicketTest.Api
```

Alternatively, run `TicketTest.Api` from Visual Studio.

The API uses the supplied SQLite database. Swagger is available when
running in the Development environment.

### Angular Client

The UI is implemented as a separate Angular application under
`TicketTest.Client`.

Install dependencies:

``` powershell
cd TicketTest.Client
npm install
```

Run the application:

``` powershell
ng serve
```

Then browse to `http://localhost:4200`.

The API URL is configured in
`TicketTest.Client/src/environments/environment.ts`. If the API is
running on a different HTTPS port, update `apiBaseUrl` accordingly.

The ASP.NET Core application includes a CORS policy allowing the Angular
development server at `http://localhost:4200`.


## 
## 1. What I changed in the starter architecture and why

I retained the existing ASP.NET Core, EF Core and SQLite structure and extended it rather than introducing additional architectural layers.

The API was expanded to support server-side filtering, searching, paging and sorting, together with create, update and delete operations. Request/response contracts were separated from the EF Core entity, reusable business rules were moved into `TicketRules`, and API errors were standardised using `ProblemDetails` / `ValidationProblemDetails`.

I added an integration test project using `WebApplicationFactory` and in-memory SQLite to test the API through its HTTP boundary.

For the UI I created a separate Angular client rather than extending the supplied React application, as permitted by the assessment. HTTP access is centralised in `TicketApiService`, while Angular Material and reactive forms are used for the UI.

I deliberately kept the architecture simple and close to the starter solution because additional repository, mediator or application layers would add little value for an application of this size.

## 2. How I would deploy this solution to Azure for production

I would use Azure App Service on Linux for the ASP.NET Core API and Azure Static Web Apps for the Angular application.

Azure SQL Database would replace SQLite. Application configuration would use App Service configuration, with secrets stored in Azure Key Vault and accessed using Managed Identity.

Application Insights and Log Analytics would provide application telemetry, distributed diagnostics, metrics and centralised logging.

I would provision the Azure infrastructure using Bicep and use Azure DevOps YAML pipelines for CI/CD. The pipeline would build and test the .NET API, build the Angular client, deploy infrastructure changes and then deploy the application components.

Separate development, test and production environments would use environment-specific configuration and deployment controls.

## 3. Adapting SQLite to Azure SQL / SQL Server

I would change the EF Core provider from SQLite to `Microsoft.EntityFrameworkCore.SqlServer` and use EF Core migrations to manage the Azure SQL schema.

I would revisit SQL Server-specific concerns rather than assuming behaviour is identical to SQLite, particularly:

- data types and date/time precision;
- indexes based on actual production query patterns;
- execution plans and query performance;
- case sensitivity and database collation;
- connection resiliency and transient-fault handling;
- connection pooling;
- migration/deployment strategy;
- database-level optimistic concurrency.

For production SQL Server I would strongly consider replacing the application's integer concurrency mechanism with SQL Server `rowversion`.

## 4. Securing the API and UI

I would use Microsoft Entra ID for authentication.

The Angular application would use MSAL to authenticate users and obtain an access token for the API. The ASP.NET Core API would validate Entra-issued JWT bearer tokens and use claims, application roles or policies to authorise operations.

The API would enforce authorisation independently of the UI; hiding or disabling UI controls would not be treated as a security boundary.

Managed Identity would be used for API access to supported Azure resources such as Key Vault, avoiding application credentials where possible. Production CORS would allow only the deployed UI origin and HTTPS would be required throughout.

## 5. Concurrency approach and limitations

Each ticket has an integer `Version`. The client receives the current Version when reading a ticket and must return it when updating or deleting it.

The API compares the supplied Version with the current database value. A mismatch means another operation has modified the ticket, so the API returns `409 Conflict`. Successful updates increment Version.

This provides straightforward optimistic concurrency for the exercise, but the read/compare/update sequence is not an atomic database-level concurrency guarantee. Two requests could potentially read the same Version before either commits.

For Azure SQL / SQL Server I would use a `rowversion` concurrency token through EF Core and handle `DbUpdateConcurrencyException`, providing stronger database-enforced protection against concurrent writes.

## 6. Performance/scalability risk as ticket volume grows

The ticket-list endpoint is the first area I would monitor.

It combines filtering, free-text search, sorting, a total-count query and paging. As the Tickets table grows, searches such as `Title.Contains()` / `Description.Contains()` and unsupported filter/sort combinations could cause increasingly expensive scans and sorts.

I would monitor query duration and execution plans through Application Insights and Azure SQL monitoring, then introduce indexes based on observed access patterns. At substantially larger scale I would also reassess the free-text search strategy rather than relying indefinitely on SQL `LIKE` queries.

## 7. What I deliberately did not build

I deliberately did not introduce repository, mediator/CQRS or additional application/domain-service layers.

Those patterns can be appropriate in a larger system, but this exercise contains a small domain and a single EF Core-backed API. Adding them would increase the amount of abstraction and code without solving a current complexity problem.

I kept EF Core access in the controller and extracted only business logic that had a clear reason to be reused. If the application grew in functionality or domain complexity, I would revisit those boundaries.

## 8. Use of AI Resources

I used AI-assisted development tools as a supporting resource during the assessment, primarily to discuss implementation approaches, review code, troubleshoot specific issues and identify areas that warranted additional testing or consideration.

I treated AI-generated suggestions in the same way I would treat other external technical guidance: as input to be reviewed rather than accepted without validation. I remained responsible for the implementation decisions and ensured that I understood the code included in the final solution.

I validated the resulting work by:

- reviewing generated or suggested code before incorporating it;
- building and running the API and Angular application locally;
- exercising the API and UI manually, including validation and error scenarios;
- writing and running automated integration tests against the API;
- manually testing create, update and delete workflows;
- testing optimistic-concurrency behaviour, including stale-version `409 Conflict` responses;
- running the completed SQL against the expected SQLite syntax and reviewing SQL Server-specific differences;
- running `dotnet test` and `ng build` as final verification.

AI assistance accelerated implementation and provided an additional review mechanism, but the submitted design decisions, code selection and validation remained my responsibility.

The content of this file was provided to an AI tool to verify, grammer & spell check and generally polish the text.

## 9. The optional bonus task - Azure AI / AI foundry

I did not have sufficent time to explore this extra work although I feel it would have increased the quality of my submission. My intention is to spend time expanding this project and add it to my reference appliciations hosted under https://ausdatatech.com.au/. 

It will be expanded to include: 

- Authentication via Azure Entra
- The AI Foundry integration described in the requirements document
- Bicep files for creating Azure resources on my Azure tenant to use for hosting
- YAML based pipelines for CI/CD
- Online documentation including a README.md with setup and run instructions
