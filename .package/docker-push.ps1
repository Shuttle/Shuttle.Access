# List of container names
$containers = @(
    "shuttle/access-server",
    "shuttle/access-webapi",
    "shuttle/access-sqlserver-linux"
)

# Prompt for version input
$version = Read-Host "Enter the version tag (e.g., 1.0.0)"

# Check if version is empty
if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host "ERROR: No version tag provided." -ForegroundColor Red
    exit 1
}

# Check if version contains hyphen (pre-release indicator)
if ($version -match '-') {
    $isPreRelease = $true
    Write-Host "Pre-release version detected ($version). Skipping 'latest' tags." -ForegroundColor Yellow
} else {
    $isPreRelease = $false
    Write-Host "Stable release detected. Will push 'latest' tags." -ForegroundColor Green
}

Write-Host ""

# Loop through each container and push with version tag
foreach ($container in $containers) {
    Write-Host "Pushing Docker image ${container}:$version..." -ForegroundColor Cyan
    docker push "${container}:$version"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to push Docker image ${container}:$version." -ForegroundColor Red
        exit 1
    }
    
    # Only push latest tag if not a pre-release
    if (-not $isPreRelease) {
        Write-Host "Tagging and pushing ${container}:latest..." -ForegroundColor Cyan
        docker tag "${container}:$version" "${container}:latest"
        docker push "${container}:latest"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ERROR: Failed to push Docker image ${container}:latest." -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host ""
}

Write-Host "All Docker images pushed successfully!" -ForegroundColor Green