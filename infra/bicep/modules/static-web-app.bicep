targetScope = 'resourceGroup'

// ============================================================================
// Parameters
// ============================================================================

@description('Location of the Static Web App.')
param location string

@description('Globally unique name of the Ticketing Static Web App.')
param staticWebAppName string

@description('Tags applied to application-owned resources.')
param tags object

// ============================================================================
// Ticketing Static Web App
// ============================================================================

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  tags: tags

  sku: {
    name: 'Free'
    tier: 'Free'
  }

  properties: {}
}

// ============================================================================
// Outputs
// ============================================================================

output staticWebAppName string = staticWebApp.name
output staticWebAppDefaultHostName string = staticWebApp.properties.defaultHostname