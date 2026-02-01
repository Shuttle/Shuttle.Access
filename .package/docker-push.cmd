@echo off
setlocal enabledelayedexpansion

:: List of container names
set containers=shuttle/access-server shuttle/access-webapi shuttle/access-sqlserver-linux

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

:: Loop through each container and push
for %%c in (%containers%) do (
    echo Pushing Docker image %%c:%version%...
    docker push %%c:%version%

    if errorlevel 1 (
        echo ERROR: Failed to push Docker image %%c:%version%.
        exit /b 1
    )

    if !pushLatest! equ 1 (
        echo Pushing Docker image %%c:latest...
        docker tag %%c:%version% %%c:latest
        docker push %%c:latest

        if errorlevel 1 (
            echo ERROR: Failed to push Docker image %%c:latest.
            exit /b 1
        )
    )
)

echo All Docker images pushed successfully!
