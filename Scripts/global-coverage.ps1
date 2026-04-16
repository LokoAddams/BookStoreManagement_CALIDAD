$ErrorActionPreference = 'Stop'

# Resuelve la raiz del repositorio a partir de la ubicacion del script.
$RepoRoot = Split-Path -Parent $PSScriptRoot
$SolutionPath = Join-Path $RepoRoot 'Microservices.Orchestrator.sln'
$SalesTestProject = Join-Path $RepoRoot 'MicroServiceSales\MicroServiceSales.Tests\MicroServiceSales.Tests.csproj'
$ResultsDir = Join-Path $RepoRoot 'TestResults'
$ReportDir = Join-Path $RepoRoot 'GlobalCoverageReport'

# Asegura que el binario global de .NET Tools quede disponible en esta sesion.
$DotnetToolsPath = Join-Path $env:USERPROFILE '.dotnet\tools'
if (Test-Path $DotnetToolsPath) {
  $env:PATH = "$DotnetToolsPath;$env:PATH"
}

if (-not (Test-Path $SolutionPath)) {
  throw "No se encontro la solucion en: $SolutionPath"
}

if (-not (Test-Path $SalesTestProject)) {
  throw "No se encontro el proyecto de pruebas de Sales en: $SalesTestProject"
}

# Genera una corrida limpia para evitar mezclar artefactos anteriores.
if (Test-Path $ResultsDir) {
  Remove-Item $ResultsDir -Recurse -Force
}
if (Test-Path $ReportDir) {
  Remove-Item $ReportDir -Recurse -Force
}
New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null
New-Item -ItemType Directory -Path $ReportDir -Force | Out-Null

# Instala el generador de reportes solo si aun no esta disponible.
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
  dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Ejecuta toda la solucion y centraliza los resultados de cobertura en la raiz.
dotnet test $SolutionPath `
  --collect:"XPlat Code Coverage" `
  --results-directory $ResultsDir

# Ejecuta explicitamente los tests de Sales para incluir su cobertura global.
dotnet test $SalesTestProject `
  --collect:"XPlat Code Coverage" `
  --results-directory $ResultsDir

# Fusiona todos los coverage.cobertura.xml generados por la solucion en un unico HTML.
reportgenerator `
  "-reports:$ResultsDir\**\coverage.cobertura.xml" `
  "-targetdir:$ReportDir" `
  -reporttypes:Html

Write-Host "Reporte generado en: $ReportDir\index.html"