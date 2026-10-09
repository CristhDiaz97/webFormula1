@echo off
setlocal enabledelayedexpansion

cd /d "%~dp0src\WebFormula1.Api"

echo.
echo ====================================
echo  🏎️  Web Formula 1 - API
echo ====================================
echo.
echo Iniciando API en modo Debug...
echo URL: https://localhost:7001
echo Swagger: https://localhost:7001/swagger
echo Hangfire: https://localhost:7001/hangfire
echo.
echo Presiona Ctrl+C para detener
echo.

dotnet run
