# Start-LocalDev.ps1
# Helper script to start the local development environment

$ErrorActionPreference = "Stop"
$rootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$wwwDir = Join-Path $rootDir "www"
$apiDir = Join-Path $wwwDir "api"

# Function to check if a port is in use
function Test-PortInUse {
    param (
        [int]$Port
    )
    $connections = netstat -aon | findstr ":$Port"
    if ($connections) {
        return $true
    }
    return $false
}

# Check for Azure Functions CLI
$funcCmd = Get-Command func -ErrorAction SilentlyContinue
if (-not $funcCmd) {
    Write-Host "Azure Functions Core Tools not found. Please install it from: https://github.com/Azure/azure-functions-core-tools" -ForegroundColor Red
    exit 1
}

# Check if ports are already in use
if (Test-PortInUse -Port 7071) {
    Write-Host "Port 7071 is already in use. Please free up this port for the API." -ForegroundColor Red
    exit 1
}
if (Test-PortInUse -Port 8080) {
    Write-Host "Port 8080 is already in use. Please free up this port for the frontend." -ForegroundColor Red
    exit 1
}

# Function to create a simple HTTP server for the frontend
function Start-SimpleHttpServer {
    param (
        [string]$RootDirectory,
        [int]$Port = 8080
    )
    
    Push-Location $RootDirectory
    
    Write-Host "Starting simple HTTP server on http://localhost:$Port" -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
    
    $listener = New-Object System.Net.HttpListener
    $listener.Prefixes.Add("http://localhost:$Port/")
    $listener.Start()
    
    try {
        while ($listener.IsListening) {
            $context = $listener.GetContext()
            $request = $context.Request
            $response = $context.Response
            
            $localPath = $request.Url.LocalPath.TrimStart('/')
            if ($localPath -eq '') { $localPath = 'index.html' }
            
            $filePath = Join-Path $RootDirectory $localPath
            
            if (Test-Path -Path $filePath -PathType Leaf) {
                $content = [System.IO.File]::ReadAllBytes($filePath)
                $response.ContentLength64 = $content.Length
                
                # Set content type based on file extension
                $extension = [System.IO.Path]::GetExtension($filePath).ToLower()
                switch ($extension) {
                    '.html' { $response.ContentType = 'text/html' }
                    '.css'  { $response.ContentType = 'text/css' }
                    '.js'   { $response.ContentType = 'application/javascript' }
                    '.json' { $response.ContentType = 'application/json' }
                    '.png'  { $response.ContentType = 'image/png' }
                    '.jpg'  { $response.ContentType = 'image/jpeg' }
                    '.gif'  { $response.ContentType = 'image/gif' }
                    default { $response.ContentType = 'application/octet-stream' }
                }
                
                $response.OutputStream.Write($content, 0, $content.Length)
            }
            else {
                $response.StatusCode = 404
                $response.ContentType = 'text/plain'
                $notFoundMessage = [System.Text.Encoding]::UTF8.GetBytes("404 - File not found: $localPath")
                $response.ContentLength64 = $notFoundMessage.Length
                $response.OutputStream.Write($notFoundMessage, 0, $notFoundMessage.Length)
            }
            
            $response.Close()
        }
    }
    finally {
        $listener.Stop()
        Pop-Location
    }
}

# Start processes
try {
    # Start API in a new process
    Start-Process -FilePath "pwsh" -ArgumentList "-Command", "cd '$apiDir'; func start" -NoNewWindow

    # Allow time for the API to start
    Write-Host "Starting API at http://localhost:7071" -ForegroundColor Green
    Start-Sleep -Seconds 5

    # Start frontend server
    Start-SimpleHttpServer -RootDirectory $wwwDir -Port 8080
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
