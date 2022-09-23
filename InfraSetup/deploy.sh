baseline=$(cat /dev/urandom | tr -dc '[:lower:]' | fold -w 4 | head -n 1)
location="northeurope"
# Create deployment
az deployment sub create -n "Dep-$baseline" -l $location -f main.bicep --parameters baseName=$baseline --parameters location=$location