param (
    [Parameter(Mandatory = $true)]
    [string]$location
)

# Create the deployment
az deployment sub create --location $location --template-file .\bicep\main.bicep --parameters .\bicep\main.bicepparam