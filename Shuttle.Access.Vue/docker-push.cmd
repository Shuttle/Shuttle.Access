@echo off
setlocal

:: Prompt for version input
set /p version="Enter the version tag (e.g., 1.0.0): "

:: Check if version is empty
if "%version%"=="" (
    echo ERROR: No version tag provided.
    exit /b 1
)

echo Pushing Docker image shuttle/access-vue:%version%...
docker push shuttle/access-vue:%version%

:: Check if the docker push command was successful
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to push Docker image shuttle/access-vue:%version%.
    exit /b 1
)

echo Pushing Docker image shuttle/access-vue:latest...
docker tag shuttle/access-vue:%version% shuttle/access-vue:latest
docker push shuttle/access-vue:latest

:: Check if the docker push command was successful
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to push Docker image shuttle/access-vue:latest.
    exit /b 1
)

echo All Docker images pushed successfully!
