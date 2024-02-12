$baseline="ors"
$location="swedencentral"

# Create deployment
az deployment sub create -n "Dep-$baseline" -l $location -f main.bicep --parameters baseName=$baseline --parameters location=$location