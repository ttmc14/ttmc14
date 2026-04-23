# PowerShell script for running the MC Entity Translator
# Запуск конвертации прототипов

$ErrorActionPreference = "Stop"
chcp 65001 | Out-Null

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MC Entity Translator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check for Python (try 'python' first, then 'py')
$pythonCmd = $null
if (Get-Command python -ErrorAction SilentlyContinue) {
    $pythonCmd = "python"
    Write-Host "[OK] Python found (python)" -ForegroundColor Green
} elseif (Get-Command py -ErrorAction SilentlyContinue) {
    $pythonCmd = "py"
    Write-Host "[OK] Python found (py)" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Python not found. Install Python 3.8 or higher." -ForegroundColor Red
    exit 1
}

# Get Python version
try {
    $pythonVersion = & $pythonCmd --version 2>&1
    Write-Host "[OK] Python version: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Could not get Python version" -ForegroundColor Red
    exit 1
}

# Install dependencies
Write-Host ""
Write-Host "[1/2] Installing dependencies..." -ForegroundColor Yellow
try {
    & $pythonCmd -m pip install -r requirements.txt | Out-Null
    Write-Host "[OK] Dependencies installed" -ForegroundColor Green
} catch {
    Write-Host "[ERROR] Failed to install dependencies" -ForegroundColor Red
    exit 1
}

# Run translator
Write-Host ""
Write-Host "[2/2] Running prototype conversion..." -ForegroundColor Yellow
try {
    & $pythonCmd translator.py
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Conversion completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} catch {
    Write-Host ""
    Write-Host "[ERROR] Script failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
pause
