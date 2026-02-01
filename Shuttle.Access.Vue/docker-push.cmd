@echo off
setlocal enabledelayedexpansion

:: Prompt for version input
set /p version="Enter the version tag (e.g., 1.0.0): "

:: Check if version is empty
if "%version%"=="" (
    echo ERROR: No version tag provided.
    exit /b 1
)

:: Detect pre-release (contains '-')
set pushLatest=1
if not "%version:-=%"=="%version%" (
    echo Pre-release version detected (%version%). Skipping 'latest' tag.
    set pushLatest=0
)

echo Pushing Docker image shuttle/access-vue:%version%...
docker push shuttle/access-vue:%version%

if errorlevel 1 (
    echo ERROR: Failed to push Docker image shuttle/access-vue:%version%.
    exit /b 1
)

if !pushLatest! equ 1 (
    echo Pushing Docker image shuttle/access-vue:latest...
    docker tag shuttle/access-vue:%version% shuttle/access-vue:latest
    docker push shuttle/access-vue:latest

    if errorlevel 1 (
        echo ERROR: Failed to push Docker image shuttle/access-vue:latest.
        exit /b 1
    )
)

echo All Docker images pushed successfully!
