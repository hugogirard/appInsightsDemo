targetScope = 'subscription'

@description('Location where all resources will be created')
@allowed([
  'eastus'
  'canadacentral'
  'canadaeast'
])
param location string

@description('Resource group name must be between 4 and 20 characters.')
@minLength(4)
@maxLength(20)
param rgName string

@description('Storage account name must be between 3 and 24 characters.')
@minLength(3)
@maxLength(24)
param storageAccountName string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: rgName
  location: location
}

var suffix = uniqueString(resourceGroup.id)

module cosmosdb 'modules/cosmosdb/cosmosdb.bicep' = {
  scope: resourceGroup
  name: 'cosmosdb'
  params: {
    location: location
    suffix: suffix
  }
}

module storage 'modules/storage/storage.bicep' = {
  scope: resourceGroup
  name: 'storage'
  params: {
    storageAccountName: storageAccountName
    location: location
  }
}

module monitoring 'modules/monitoring/monitoring.bicep' = {
  scope: resourceGroup
  name: 'monitoring'
  params: {
    location: location
    suffix: suffix
  }
}

module redis 'modules/redis/redis.bicep' = {
  scope: resourceGroup
  name: 'redis'
  params: {
    location: location
    suffix: suffix
  }
}

module web 'modules/webapp/webapp.bicep' = {
  scope: resourceGroup
  name: 'web'
  params: {
    location: location
    appInsightName: monitoring.outputs.appInsightName
    suffix: suffix
  }
}
