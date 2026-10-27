@echo off
setlocal
cd /d "%~dp0"

echo [1/3] Stopping the previous run...
docker compose down
if errorlevel 1 exit /b 1
for %%C in (tasktracker-api tasktracker-worker tasktracker-notifications tasktracker-postgres tasktracker-prometheus tasktracker-grafana tasktracker-jaeger) do docker rm --force %%C >nul 2>&1

echo [2/3] Building and starting TaskTracker...
docker compose up --build --force-recreate --detach --wait --wait-timeout 180
if errorlevel 1 (
    echo Startup failed. Recent container logs:
    docker compose logs --tail=100
    exit /b 1
)

echo [3/3] Waiting for API health. PostgreSQL is healthy; the API applies migrations before listening.
for /L %%i in (1,1,60) do (
    powershell -NoProfile -Command "try { Invoke-WebRequest -Uri 'http://localhost:8080/health/ready' -TimeoutSec 2 -UseBasicParsing | Out-Null; exit 0 } catch { exit 1 }"
    if not errorlevel 1 goto api_ready
    timeout /t 2 /nobreak >nul
)

echo API did not become healthy. Recent API logs:
docker compose logs --tail=100 api
exit /b 1

:api_ready
echo Swagger UI: http://localhost:8080/swagger
echo API health: http://localhost:8080/health/ready

echo Showing application logs. Press Ctrl+C to stop following logs.
docker compose logs --follow api worker notifications
