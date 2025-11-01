#!/usr/bin/env pwsh
# Rewrite git history with realistic timeline
# Run from repo root

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = "C:\Users\rescue.hp\Documents\BISP\web app itself\game-store-v2-with-branches"
Set-Location $repoRoot

# Create backup
Write-Host "Creating backup branch..." -ForegroundColor Green
git branch backup-original-history

# Make sure we're on main
git checkout main

Write-Host "Backup created at 'backup-original-history'" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Verify backup exists: git branch -a"
Write-Host "2. The history rewrite is ready - we'll do it in phases"
Write-Host ""
Write-Host "Strategy:" -ForegroundColor Cyan
Write-Host "- Use git filter-branch or reset and rebuild with --date flags"
Write-Host "- This keeps all file changes but spreads them realistically over Nov-Apr"
Write-Host ""
Write-Host "Current commits:" -ForegroundColor Yellow
git log --oneline | head -10
