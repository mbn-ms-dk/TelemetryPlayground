param baseName string
param location string

resource la 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: 'law-${baseName}'
  location: location
}

output lawId string = la.id
output lawName string = la.name
