targetScope = 'subscription'

//parameters
param baseName string
param location string

//variables
var rgName = 'rg-${baseName}-otel'


//Modules
//Resource-Group
module rg 'modules/resource-group/rg.bicep' = {
  name: rgName
  params: {
    rgName: rgName
    location: location
  }
}

//LA Workspace
module laws 'modules/laworkspace/la.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'la-ws'
  params: {
    baseName: baseName
    location: location
  }
}

//Application Insights
module app 'modules/applicationinsights/appins.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'appins'
  params: {
    baseName: baseName
    location: location
    lawsId: laws.outputs.lawId
  }
}
