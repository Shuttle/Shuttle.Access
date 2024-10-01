@echo off
setlocal

:: List of container names
set containers=shuttle/access-server shuttle/access-projection shuttle/access-webapi shuttle/access-sqlserver-linux

:: Prompt for version input
set /p version="Enter the version tag (e.g., 1.0.0): "

:: Check if version is empty
if "%version%"=="" (
    echo ERROR: No version tag provided.
    exit /b 1
)

:: Loop through each container and push with version and latest tags
for %%c in (%containers%) do (
    echo Pushing Docker image %%c:%version%...
    docker push %%c:%version%
    
    :: Check if the docker push command was successful
    if %ERRORLEVEL% neq 0 (
        echo ERROR: Failed to push Docker image %%c:%version%.
        exit /b 1
    )
    
    echo Pushing Docker image %%c:latest...
    docker tag %%c:%version% %%c:latest
    docker push %%c:latest
    
    :: Check if the docker push command was successful
    if %ERRORLEVEL% neq 0 (
        echo ERROR: Failed to push Docker image %%c:latest.
        exit /b 1
    )
)

echo All Docker images pushed successfully!
