# Drashyam Project Cleanup Script
# Run this if Cursor is still timing out

Write-Host "Cleaning Drashyam project for Cursor performance..." -ForegroundColor Green

# Clean Angular cache
if (Test-Path "Frontend\.angular") {
    Remove-Item -Path "Frontend\.angular" -Recurse -Force
    Write-Host "Removed Angular cache" -ForegroundColor Yellow
}

# Clean .NET build artifacts
if (Test-Path "Backend\Drashyam.API\bin") {
    Remove-Item -Path "Backend\Drashyam.API\bin" -Recurse -Force
    Write-Host "Removed .NET bin directory" -ForegroundColor Yellow
}

if (Test-Path "Backend\Drashyam.API\obj") {
    Remove-Item -Path "Backend\Drashyam.API\obj" -Recurse -Force
    Write-Host "Removed .NET obj directory" -ForegroundColor Yellow
}

# Clean Git repository
Write-Host "Optimizing Git repository..." -ForegroundColor Yellow
git gc --aggressive --prune=now
git repack -a -d --depth=250 --window=250

Write-Host "Cleanup complete! Please restart Cursor IDE." -ForegroundColor Green
Write-Host "If issues persist, try opening only the Backend or Frontend folder separately." -ForegroundColor Cyan
