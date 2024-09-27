param suffix string
param location string

resource redisCache 'Microsoft.Cache/Redis@2020-06-01' = {
  name: 'redis-${suffix}'
  location: location
  properties: {
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    sku: {
      capacity: 1
      family: 'C'
      name: 'Basic'
    }
  }
}
