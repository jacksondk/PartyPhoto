# Deploy-StaticWebApp.ps1
# Helper script to create and deploy to an Azure Static Web App

param (
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus2",
    
    [Parameter(Mandatory=$false)]
    [string]$RepositoryUrl,
    
    [Parameter(Mandatory=$false)]
    [string]$Branch = "main"
)

$ErrorActionPreference = "Stop"

# Check if Azure CLI is installed
$azCmd = Get-Command az -ErrorAction SilentlyContinue
if (-not $azCmd) {
    Write-Host "Azure CLI not found. Please install it from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# Ensure logged in to Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$loginStatus = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Not logged in to Azure. Initiating login..." -ForegroundColor Yellow
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to login to Azure." -ForegroundColor Red
        exit 1
    }
}

# Check if resource group exists
Write-Host "Checking if resource group '$ResourceGroupName' exists..." -ForegroundColor Yellow
$resourceGroupExists = az group exists --name $ResourceGroupName
if ($resourceGroupExists -eq "false") {
    Write-Host "Resource group '$ResourceGroupName' does not exist. Creating..." -ForegroundColor Yellow
    az group create --name $ResourceGroupName --location $Location
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create resource group." -ForegroundColor Red
        exit 1
    }
}

# Create Static Web App
Write-Host "Creating/updating Static Web App '$AppName'..." -ForegroundColor Yellow

$staticWebAppParams = @(
    "--name", $AppName,
    "--resource-group", $ResourceGroupName,
    "--location", $Location,
    "--sku", "Free"
)

if ($RepositoryUrl) {
    $staticWebAppParams += @(
        "--source", $RepositoryUrl,
        "--branch", $Branch,
        "--app-location", "www",
        "--api-location", "www/api",
        "--output-location", ""
    )
}

az staticwebapp create $staticWebAppParams
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to create/update Static Web App." -ForegroundColor Red
    exit 1
}

# Get the deployment token for GitHub Actions
$deploymentToken = az staticwebapp secrets list --name $AppName --resource-group $ResourceGroupName --query "properties.apiKey" -o tsv
Write-Host "Successfully created/updated Static Web App '$AppName'." -ForegroundColor Green
Write-Host ""
Write-Host "Deployment Information:" -ForegroundColor Green
Write-Host "------------------------" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Green
Write-Host "Static Web App: $AppName" -ForegroundColor Green
Write-Host "Location: $Location" -ForegroundColor Green
Write-Host ""
Write-Host "Deployment Token for GitHub Actions:" -ForegroundColor Yellow
Write-Host $deploymentToken -ForegroundColor Yellow
Write-Host ""
Write-Host "Add this token to your GitHub repository as a secret named 'AZURE_STATIC_WEB_APPS_API_TOKEN'" -ForegroundColor Yellow
Write-Host ""
Write-Host "Access your Static Web App in the Azure Portal:" -ForegroundColor Green
Write-Host "https://portal.azure.com/#resource/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$ResourceGroupName/providers/Microsoft.Web/staticSites/$AppName" -ForegroundColor Green
