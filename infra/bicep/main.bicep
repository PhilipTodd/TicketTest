targetScope = 'subscription'

// ============================================================================
// Deployment
// ============================================================================

@description('Deployment environment.')
@allowed([
  'dev'
  'test'
  'prod'
])
param environment string = 'dev'

@description('Location of the main Ticketing application resources.')
param applicationLocation string
@description('Location of the static web app resource.')
param staticWebAppLocation string

@description('Name of the Ticketing application resource group.')
param applicationResourceGroupName string

@description('Tags applied to application-owned resources.')
param tags object = {
  environment: environment
  managedBy: 'bicep'
  project: 'ticketing-reference'
  capability: 'ticketing'
}

// ============================================================================
// Shared platform resources
// ============================================================================

@description('Name of the shared platform resource group.')
param platformResourceGroupName string

@description('Name of the shared Linux App Service Plan.')
param appServicePlanName string

@description('Name of the shared Application Insights resource.')
param applicationInsightsName string

@description('Name of the shared Log Analytics workspace.')
param logAnalyticsWorkspaceName string

@description('Name of the shared Azure SQL logical server.')
param sqlServerName string

@description('Name of the shared Azure SQL database.')
param sqlDatabaseName string

// ============================================================================
// Application resources
// ============================================================================

@description('Globally unique name of the Ticketing API App Service.')
param apiAppName string

@description('Globally unique name of the Ticketing Static Web App.')
param staticWebAppName string

// ============================================================================
// CORS
// ============================================================================

@description('Allowed browser origins for the API.')
param corsAllowedOrigins array

// ============================================================================
// Connection strings
//
// Retained during the initial Azure hosting implementation. Migration to
// managed identity authentication for Azure SQL can be performed separately.
// ============================================================================

@secure()
@description('Connection string for ReferenceProjectsDb.')
param sqlConnectionString string

// ============================================================================
// Application resource group
// ============================================================================

resource applicationResourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: applicationResourceGroupName
  location: applicationLocation
  tags: tags
}

// ============================================================================
// Shared platform resources
// ============================================================================

resource sharedAppServicePlan 'Microsoft.Web/serverfarms@2024-04-01' existing = {
  name: appServicePlanName
  scope: resourceGroup(platformResourceGroupName)
}

resource sharedApplicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
  scope: resourceGroup(platformResourceGroupName)
}

resource sharedLogAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logAnalyticsWorkspaceName
  scope: resourceGroup(platformResourceGroupName)
}

resource sharedSqlServer 'Microsoft.Sql/servers@2023-08-01' existing = {
  name: sqlServerName
  scope: resourceGroup(platformResourceGroupName)
}

resource sharedSqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' existing = {
  parent: sharedSqlServer
  name: sqlDatabaseName
}

// ============================================================================
// Ticketing Static Web App
// ============================================================================

module staticWebApp './modules/static-web-app.bicep' = {
  name: 'ticketing-static-web-app'
  scope: applicationResourceGroup

  params: {
    location: staticWebAppLocation
    staticWebAppName: staticWebAppName
    tags: tags
  }
}

// ============================================================================
// Ticketing API App Service
// ============================================================================

module appService './modules/appservice.bicep' = {
  name: 'ticketing-appservice'
  scope: applicationResourceGroup

  params: {
    location: applicationLocation

    appServicePlanId: sharedAppServicePlan.id

    apiAppName: apiAppName

    applicationInsightsConnectionString: sharedApplicationInsights.properties.ConnectionString

    sqlConnectionString: sqlConnectionString

    corsAllowedOrigins: concat(
      corsAllowedOrigins,
      [
        'https://${staticWebApp.outputs.staticWebAppDefaultHostName}'
      ]
    )

    environment: environment
    tags: tags
  }
}

// ============================================================================
// Outputs
// ============================================================================

output applicationResourceGroupName string = applicationResourceGroup.name
output applicationResourceGroupId string = applicationResourceGroup.id

output sharedAppServicePlanName string = sharedAppServicePlan.name
output sharedAppServicePlanId string = sharedAppServicePlan.id

output sharedApplicationInsightsName string = sharedApplicationInsights.name
output sharedLogAnalyticsWorkspaceName string = sharedLogAnalyticsWorkspace.name

output sharedSqlServerName string = sharedSqlServer.name
output sharedSqlDatabaseName string = sharedSqlDatabase.name

output apiAppName string = appService.outputs.apiAppName
output apiAppDefaultHostName string = appService.outputs.apiAppDefaultHostName
output apiAppPrincipalId string = appService.outputs.apiAppPrincipalId

output staticWebAppName string = staticWebApp.outputs.staticWebAppName
output staticWebAppDefaultHostName string = staticWebApp.outputs.staticWebAppDefaultHostName