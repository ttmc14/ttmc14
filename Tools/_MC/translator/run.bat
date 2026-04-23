@echo off
chcp 65001 >nul
echo ========================================
echo MC Entity Translator
echo ========================================
echo.

echo Checking Python...
set PYTHON_CMD=

rem Try 'python' first
python --version >nul 2>&1
if %errorlevel% equ 0 (
    set PYTHON_CMD=python
    echo Python found: python
)

rem If 'python' doesn't work, try 'py'
if "%PYTHON_CMD%"=="" (
    py --version >nul 2>&1
    if %errorlevel% equ 0 (
        set PYTHON_CMD=py
        echo Python found: py
    )
)

if "%PYTHON_CMD%"=="" (
    echo ERROR: Python not found
    echo Install Python 3.8+ from https://www.python.org/downloads/
    pause
    exit /b 1
)

echo Using: %PYTHON_CMD%
echo.

echo [1/2] Installing dependencies...
%PYTHON_CMD% -m pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo ERROR: Failed to install dependencies
    pause
    exit /b 1
)
echo Dependencies installed successfully
echo.

echo [2/2] Running prototype conversion...
%PYTHON_CMD% translator.py
if %errorlevel% neq 0 (
    echo ERROR: Script failed
    pause
    exit /b 1
)
echo.
echo ========================================
echo Conversion completed successfully!
echo ========================================
pause
