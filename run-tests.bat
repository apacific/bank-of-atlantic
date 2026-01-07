@echo off
REM ============================================================================
REM Banking POC - E2E Testing Helper Script
REM This script helps set up and run the full E2E testing environment
REM ============================================================================

setlocal enabledelayedexpansion

:menu
cls
echo.
echo ========================================
echo    Banking POC - E2E Testing Helper
echo ========================================
echo.
echo Choose an operation:
echo.
echo 1. Start PostgreSQL database
echo 2. Start Backend API
echo 3. Start Frontend (in new window)
echo 4. Run E2E tests
echo 5. Check service status
echo 6. Stop all services
echo 7. Build solution
echo 8. Run all tests (backend + frontend + e2e)
echo 9. Exit
echo.
set /p choice="Enter your choice (1-9): "

if "%choice%"=="1" goto start_db
if "%choice%"=="2" goto start_api
if "%choice%"=="3" goto start_frontend
if "%choice%"=="4" goto run_e2e
if "%choice%"=="5" goto check_status
if "%choice%"=="6" goto stop_services
if "%choice%"=="7" goto build
if "%choice%"=="8" goto run_all_tests
if "%choice%"=="9" goto exit_script

echo Invalid choice. Please try again.
timeout /t 2 >nul
goto menu

:start_db
cls
echo.
echo Starting PostgreSQL database...
echo.
docker run -d --name banking-db ^
  -e POSTGRES_USER=banking ^
  -e POSTGRES_PASSWORD=banking ^
  -e POSTGRES_DB=banking ^
  -p 5433:5432 ^
  postgres:16-alpine
echo.
echo Database container started. Waiting 5 seconds for initialization...
timeout /t 5 >nul
docker ps | findstr banking-db
if %errorlevel% equ 0 (
    echo ✓ PostgreSQL is running on port 5433
) else (
    echo ✗ Failed to start PostgreSQL
)
pause
goto menu

:start_api
cls
echo.
echo Starting Backend API...
echo.
cd /d c:\projects\software\fullstack\banking-poc\backend
echo Checking database connection...
timeout /t 2 >nul
dotnet run --project src/Banking.Api
goto menu

:start_frontend
cls
echo.
echo Starting Frontend (in new window)...
echo.
start cmd /k "cd /d c:\projects\software\fullstack\banking-poc\frontend && npm run dev"
timeout /t 3 >nul
echo Frontend started in new window at http://localhost:5173
pause
goto menu

:run_e2e
cls
echo.
echo Running E2E Tests...
echo.
echo Prerequisites check:
echo.

REM Check if database is running
docker ps | findstr banking-db >nul 2>&1
if %errorlevel% neq 0 (
    echo ✗ PostgreSQL database is NOT running
    echo   Run option 1 to start it first
    pause
    goto menu
)
echo ✓ PostgreSQL database is running

REM Check if API is responding
timeout /t 1 >nul
curl -s http://localhost:5185/health >nul 2>&1
if %errorlevel% neq 0 (
    echo ✗ Backend API is NOT responding
    echo   Run option 2 to start it first
    pause
    goto menu
)
echo ✓ Backend API is running

REM Check if Frontend is responding
curl -s http://localhost:5173/ >nul 2>&1
if %errorlevel% neq 0 (
    echo ✗ Frontend is NOT responding
    echo   Run option 3 to start it first
    pause
    goto menu
)
echo ✓ Frontend is running

echo.
echo All services verified. Running E2E tests...
echo.
cd /d c:\projects\software\fullstack\banking-poc\backend
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj -v detailed
pause
goto menu

:check_status
cls
echo.
echo ========================================
echo    Service Status Check
echo ========================================
echo.

REM Check Database
docker ps | findstr banking-db >nul 2>&1
if %errorlevel% equ 0 (
    echo [✓] PostgreSQL Database - RUNNING (Port 5433)
) else (
    echo [✗] PostgreSQL Database - NOT RUNNING
)

REM Check API
curl -s http://localhost:5185/health >nul 2>&1
if %errorlevel% equ 0 (
    echo [✓] Backend API - RUNNING (Port 5185)
) else (
    echo [✗] Backend API - NOT RUNNING (Port 5185)
)

REM Check Frontend
curl -s http://localhost:5173/ >nul 2>&1
if %errorlevel% equ 0 (
    echo [✓] Frontend - RUNNING (Port 5173)
) else (
    echo [✗] Frontend - NOT RUNNING (Port 5173)
)

echo.
echo Docker containers:
docker ps -a | findstr banking-db
echo.
pause
goto menu

:stop_services
cls
echo.
echo Stopping services...
echo.
echo Stopping Docker container...
docker stop banking-db >nul 2>&1
docker rm banking-db >nul 2>&1
echo Docker container stopped.
echo.
echo Note: Use Ctrl+C in the other terminals to stop:
echo - Backend API
echo - Frontend
echo.
pause
goto menu

:build
cls
echo.
echo Building solution...
echo.
cd /d c:\projects\software\fullstack\banking-poc\backend
dotnet build
pause
goto menu

:run_all_tests
cls
echo.
echo Running all tests (Backend + Frontend + E2E)...
echo.

echo 1. Running Backend Tests...
cd /d c:\projects\software\fullstack\banking-poc\backend
dotnet test

echo.
echo 2. Running Frontend Tests...
cd /d c:\projects\software\fullstack\banking-poc\frontend
call npm run test:unit -- --run

echo.
echo 3. Running E2E Tests...
cd /d c:\projects\software\fullstack\banking-poc\backend
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj

echo.
echo All tests completed!
pause
goto menu

:exit_script
echo.
echo Exiting Banking POC Helper...
exit /b 0
