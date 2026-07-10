metadata description = 'Container registry, deployed before main.bicep so images exist by the time the container apps reference them.'

@description('Location for the registry.')
param location string = resourceGroup().location

@description('Prefix for resource names. Must match the value passed to main.bicep.')
@minLength(3)
@maxLength(8)
param namePrefix string = 'moyo'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  // Location is part of the hash: registry names are global, and reusing one that
  // recently existed in another region leaves its name pointing at the old region.
  name: '${namePrefix}acr${uniqueString(resourceGroup().id, location)}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    // Images are pulled with a managed identity, so the admin account stays off.
    adminUserEnabled: false
  }
}

output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
