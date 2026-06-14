@echo off
echo Adding files to git...
git add -A
echo.
set /p msg="Enter commit message (or press Enter for default): "
if "%msg%"=="" set msg=Update FunAiGateway
echo.
echo Committing: %msg%
git commit -m "%msg%"
echo.
echo Pushing to origin main...
git push origin main
echo.
if %errorlevel% neq 0 (
    echo Push failed! You may need to pull first or use force push.
    pause
    exit /b 1
)
echo Push succeeded!
pause
