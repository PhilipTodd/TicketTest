targetScope = 'resourceGroup'

// ============================================================================
// Parameters
// ============================================================================

@description('Location of the application resources.')
param location string

@description('Resource ID of the shared Linux App Service Plan.')
param appServicePlanId string

@description('Globally unique name of the Ticketing API App Service.')
param apiAppName string

@description('Application Insights connection string.')
param applicationInsightsConnectionString string

@secure()
@description('Connection string for ReferenceProjectsDb.')
param sqlConnectionString string

@description('Allowed browser origins for the API.')
param corsAllowedOrigins array

@description('Deployment environment.')
param environment string

@description('Tags applied to application-owned resources.')
param tags object

// ============================================================================
// Application settings
// ============================================================================

var corsAppSettings = [
  for (origin, index) in corsAllowedOrigins: {
    name: 'Cors__AllowedOrigins__${index}'
    value: origin
  }
]

var appSettings = concat([
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: environment == 'prod' ? 'Production' : 'Development'
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: applicationInsightsConnectionString
  }
  {
    name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
    value: '~3'
  }
  {
    name: 'ConnectionStrings__DefaultConnection'
    value: sqlConnectionString
  }
], corsAppSettings)

// ============================================================================
// Ticketing API App Service
// ============================================================================

resource apiApp 'Microsoft.Web/sites@2024-04-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true

    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: appSettings
    }
  }
}

// ============================================================================
// Outputs
// ============================================================================

output apiAppName string = apiApp.name
output apiAppDefaultHostName string = apiApp.properties.defaultHostName
output apiAppPrincipalId string = apiApp.identity.principalId