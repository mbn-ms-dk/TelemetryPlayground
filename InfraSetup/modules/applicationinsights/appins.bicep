param baseName string
param location string
param lawsId string

resource appins 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appins-${baseName}'
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'other'
    RetentionInDays: 30
    WorkspaceResourceId: lawsId
  }
}

output appId string = appins.id
output appKey string = appins.properties.InstrumentationKey

