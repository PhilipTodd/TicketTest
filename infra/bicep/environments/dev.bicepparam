using '../main.bicep'

// ============================================================================
// Deployment
// ============================================================================

param environment = 'dev'

param applicationLocation = 'australiaeast'
param staticWebAppLocation = 'eastasia'

param applicationResourceGroupName = 'rg-ticketing-dev'

param tags = {
  environment: 'dev'
  managedBy: 'bicep'
  project: 'ticketing-reference'
  capability: 'ticketing'
}

// ============================================================================
// Shared platform resources
// ============================================================================

param platformResourceGroupName = 'rg-platform-dev'

param appServicePlanName = 'asp-platform-dev'

param applicationInsightsName = 'appi-platform-dev'

param logAnalyticsWorkspaceName = 'log-platform-dev'

param sqlServerName = 'sql-adt-platform-dev'

param sqlDatabaseName = 'ReferenceProjectsDb'

// ============================================================================
// Application resources
// ============================================================================

param apiAppName = 'api-adt-ticketing-dev'

param staticWebAppName = 'web-adt-ticketing-dev'

// ============================================================================
// CORS
// ============================================================================

param corsAllowedOrigins = [
  'http://localhost:4200'
]

// ============================================================================
// Connection strings
//
// Supply the secure value from the deployment pipeline rather than committing
// the connection string to source control.
// ============================================================================

// sqlConnectionString is supplied by the deployment pipeline.
param sqlConnectionString = readEnvironmentVariable('SQL_CONNECTION_STRING')
