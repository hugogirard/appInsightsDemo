param location string
param suffix string
param appInsightName string
// param cacheName string

var appPlanname = 'asp-${suffix}'

resource appservice 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appPlanname
  location: location
  properties: {}
  sku: {
    name: 'S1'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightName
}

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: 'api-${suffix}'
  kind: 'app'
  location: location
  properties: {
    serverFarmId: appservice.id
    clientAffinityEnabled: false
    siteConfig: {
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~2'
        }
      ]
      netFrameworkVersion: 'v8.0'
      use32BitWorkerProcess: false
      alwaysOn: true
    }
  }
}

// resource cache 'Microsoft.Cache/redis@2021-06-01' existing = {
//   name: cacheName
// }

// var cacheCnxString = '${cacheName}.redis.cache.windows.net:6380,password=${cache.listKeys().primaryKey},ssl=True,abortConnect=False'

// resource weatherApi 'Microsoft.Web/sites@2021-03-01' = {
//   name: 'weatherapi-${suffix}'
//   location: location
//   properties: {
//     siteConfig: {
//       linuxFxVersion: 'DOTNETCORE|8.0'
//     }
//     serverFarmId: appservice.id
//   }
// }
