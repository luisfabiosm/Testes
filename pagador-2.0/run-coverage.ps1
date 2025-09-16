# ====================================
# OPÇÃO 1: POWERSHELL (.ps1)
# Salve como: run-coverage.ps1
# ====================================

# Script PowerShell para análise completa de cobertura de testes
# PIX Pagador - Análise de Coverage

Write-Host "🔍 Iniciando análise de cobertura de testes..." -ForegroundColor Cyan

# 1. Limpar resultados anteriores
Write-Host "🧹 Limpando resultados anteriores..." -ForegroundColor Yellow
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
}

# 2. Navegar para o diretório de testes
Write-Host "📂 Navegando para diretório de testes..." -ForegroundColor Green
Set-Location "src\pix-pagador-testes"

# 3. Executar todos os testes com cobertura
Write-Host "🧪 Executando testes com cobertura..." -ForegroundColor Cyan

$testCommand = @(
    "test",
    "--collect:`"XPlat Code Coverage`"",
    "--results-directory", "TestResults",
    "--verbosity", "normal",
    "/p:CollectCoverage=true",
    "/p:CoverletOutputFormat=opencover,cobertura,json,lcov",
    "/p:CoverletOutput=TestResults/coverage",
    "/p:Threshold=85",
    "/p:ThresholdType=line,branch",
    "/p:ThresholdStat=minimum"
)

$process = Start-Process -FilePath "dotnet" -ArgumentList $testCommand -Wait -PassThru -NoNewWindow

# 4. Verificar se atingiu o threshold
if ($process.ExitCode -eq 0) {
    Write-Host "✅ Cobertura atingiu o threshold de 85%" -ForegroundColor Green
} else {
    Write-Host "❌ Cobertura abaixo do threshold de 85%" -ForegroundColor Red
}

# 5. Verificar se reportgenerator está instalado
$reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue
if ($reportGenerator) {
    Write-Host "📊 Gerando relatório HTML..." -ForegroundColor Cyan
    
    & reportgenerator `
        "-reports:TestResults\coverage.opencover.xml" `
        "-targetdir:TestResults\html-report" `
        "-reporttypes:HTML;HTMLSummary"
    
    Write-Host "📖 Relatório HTML gerado em: TestResults\html-report\index.html" -ForegroundColor Green
} else {
    Write-Host "ℹ️  Para relatório HTML, instale reportgenerator:" -ForegroundColor Yellow
    Write-Host "   dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor White
}

# 6. Mostrar resumo dos arquivos gerados
Write-Host ""
Write-Host "📁 Arquivos de cobertura gerados:" -ForegroundColor Cyan
Get-ChildItem "TestResults\coverage*" -ErrorAction SilentlyContinue | Format-Table Name, Length, LastWriteTime

Write-Host ""
Write-Host "🎯 Meta de cobertura: 85%" -ForegroundColor Yellow
Write-Host "📄 Formatos disponíveis:" -ForegroundColor Cyan
Write-Host "   - OpenCover: TestResults\coverage.opencover.xml" -ForegroundColor White
Write-Host "   - Cobertura: TestResults\coverage.cobertura.xml" -ForegroundColor White
Write-Host "   - JSON: TestResults\coverage.json" -ForegroundColor White
Write-Host "   - LCOV: TestResults\coverage.info" -ForegroundColor White

# 7. Criar script adicional para CI/CD
$ciScript = @'
# Script PowerShell para verificação de cobertura em CI/CD
Write-Host "🚀 Verificação de cobertura para CI/CD" -ForegroundColor Cyan

$process = Start-Process -FilePath "dotnet" -ArgumentList @(
    "test",
    "/p:CollectCoverage=true",
    "/p:CoverletOutputFormat=json",
    "/p:Threshold=85",
    "/p:ThresholdType=line,branch",
    "/p:ThresholdStat=minimum"
) -Wait -PassThru -NoNewWindow

if ($process.ExitCode -eq 0) {
    Write-Host "✅ BUILD SUCCESS - Cobertura adequada" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ BUILD FAILED - Cobertura insuficiente" -ForegroundColor Red
    Write-Host "📋 Verifique os testes e adicione cobertura nas áreas faltantes" -ForegroundColor Yellow
    exit 1
}
'@

Set-Content -Path "check-coverage.ps1" -Value $ciScript -Encoding UTF8

Write-Host ""
Write-Host "✨ Análise concluída!" -ForegroundColor Green
Write-Host "🔧 Script adicional criado: check-coverage.ps1" -ForegroundColor Cyan

# ====================================
# OPÇÃO 2: BATCH (.bat/.cmd)
# Salve como: run-coverage.bat
# ====================================

# @echo off
# chcp 65001 >nul
# echo 🔍 Iniciando análise de cobertura de testes...
# 
# echo 🧹 Limpando resultados anteriores...
# if exist TestResults rmdir /s /q TestResults
# 
# echo 📂 Navegando para diretório de testes...
# cd src\pix-pagador-testes
# 
# echo 🧪 Executando testes com cobertura...
# dotnet test ^
#   --collect:"XPlat Code Coverage" ^
#   --results-directory TestResults ^
#   --verbosity normal ^
#   /p:CollectCoverage=true ^
#   /p:CoverletOutputFormat=opencover,cobertura,json,lcov ^
#   /p:CoverletOutput=TestResults/coverage ^
#   /p:Threshold=85 ^
#   /p:ThresholdType=line,branch ^
#   /p:ThresholdStat=minimum
# 
# if %ERRORLEVEL% equ 0 (
#     echo ✅ Cobertura atingiu o threshold de 85%%
# ) else (
#     echo ❌ Cobertura abaixo do threshold de 85%%
# )
# 
# where reportgenerator >nul 2>&1
# if %ERRORLEVEL% equ 0 (
#     echo 📊 Gerando relatório HTML...
#     reportgenerator ^
#         -reports:"TestResults\coverage.opencover.xml" ^
#         -targetdir:"TestResults\html-report" ^
#         -reporttypes:"HTML;HTMLSummary"
#     echo 📖 Relatório HTML gerado em: TestResults\html-report\index.html
# ) else (
#     echo ℹ️  Para relatório HTML, instale reportgenerator:
#     echo    dotnet tool install -g dotnet-reportgenerator-globaltool
# )
# 
# echo.
# echo 📁 Arquivos de cobertura gerados:
# dir TestResults\coverage* /b 2>nul
# 
# echo.
# echo 🎯 Meta de cobertura: 85%%
# echo 📄 Formatos disponíveis:
# echo    - OpenCover: TestResults\coverage.opencover.xml
# echo    - Cobertura: TestResults\coverage.cobertura.xml
# echo    - JSON: TestResults\coverage.json
# echo    - LCOV: TestResults\coverage.info
# 
# pause

# ====================================
# OPÇÃO 3: BASH para WSL/Git Bash
# Salve como: run-coverage.sh
# (Mesmo script original, funciona no WSL)
# ====================================