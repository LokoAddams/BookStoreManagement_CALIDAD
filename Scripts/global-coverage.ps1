$ErrorActionPreference = 'Stop'

# Resuelve la raiz del repositorio a partir de la ubicacion del script.
$RepoRoot = Split-Path -Parent $PSScriptRoot
$SolutionPath = Join-Path $RepoRoot 'Microservices.Orchestrator.sln'
$ResultsDir = Join-Path $RepoRoot 'TestResults'
$ReportDir = Join-Path $RepoRoot 'GlobalCoverageReport'

# Asegura que el binario global de .NET Tools quede disponible en esta sesion.
$UserProfilePath = if ($env:USERPROFILE) { $env:USERPROFILE } else { $HOME }
$DotnetToolsPath = Join-Path $UserProfilePath '.dotnet/tools'
if (Test-Path $DotnetToolsPath) {
  $env:PATH = "$DotnetToolsPath;$env:PATH"
}

if (-not (Test-Path $SolutionPath)) {
  throw "No se encontro la solucion en: $SolutionPath"
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

# Fusiona todos los coverage.cobertura.xml generados por la solucion en un unico HTML detallado.
reportgenerator `
  "-reports:$ResultsDir\**\coverage.cobertura.xml" `
  "-targetdir:$ReportDir" `
  "-assemblyfilters:+*;-*.Tests;-*UnitTest" `
  -reporttypes:Html

Write-Host "Reporte generado en: $ReportDir\index.html"