$baseline="mbn"

#clean up
az group delete -g "rg-$baseline-otel"
az deployment sub delete -n "Dep-$baseline"