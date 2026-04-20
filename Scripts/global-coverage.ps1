$ErrorActionPreference = 'Stop'

# Resuelve la raiz del repositorio desde GitHub Actions o, en local, desde la ubicacion del script.
$RepoRoot = if ($env:GITHUB_WORKSPACE) { $env:GITHUB_WORKSPACE } else { Split-Path -Parent $PSScriptRoot }
$SolutionPath = Join-Path $RepoRoot 'Microservices.Orchestrator.sln'
$ResultsDir = Join-Path $RepoRoot 'TestResults'
$ReportDir = Join-Path $RepoRoot 'GlobalCoverageReport'

# Asegura que el binario global de .NET Tools quede disponible en esta sesion.
$UserProfilePath = if ($env:USERPROFILE) { $env:USERPROFILE } else { $HOME }
$DotnetToolsPath = Join-Path $UserProfilePath '.dotnet/tools'
if (Test-Path $DotnetToolsPath) {
  $PathSeparator = [System.IO.Path]::PathSeparator
  $env:PATH = "$DotnetToolsPath$PathSeparator$env:PATH"
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

# Descubre todos los archivos de cobertura y falla con mensaje claro si no existen.
$CoverageFiles = Get-ChildItem -Path $ResultsDir -Recurse -Filter 'coverage.cobertura.xml' -File
if (-not $CoverageFiles -or $CoverageFiles.Count -eq 0) {
  throw "No se encontraron archivos coverage.cobertura.xml en: $ResultsDir"
}

# Fusiona todos los coverage.cobertura.xml generados por la solucion en un unico HTML detallado.
$ReportsArg = '-reports:' + (($CoverageFiles | ForEach-Object { $_.FullName }) -join ';')
reportgenerator `
  $ReportsArg `
  "-targetdir:$ReportDir" `
  "-assemblyfilters:+*;-*.Tests;-*UnitTest" `
	"-classfilters:-MicroServiceUsers.Infrastructure.DataBase.DataBaseConnection;-MicroServiceUsers.Infrastructure.Email.SendGridEmailService;-MicroServiceUsers.Infrastructure.Email.SendGridOptions;-MicroServiceUsers.Infrastructure.Repositories.RoleRepository;-MicroServiceUsers.Infrastructure.Repositories.UserRepository;-MicroServiceUsers.Application.DTOs.ChangePasswordDto;-MicroServiceUsers.Domain.Models.PagedResult*;-MicroServiceUsers.Domain.Models.Role;-MicroServiceUsers.Domain.Validations.ValidationException" `
  "-filefilters:+*;-*ValidationError.cs" `
  "-reporttypes:Html;MarkdownSummaryGithub"

$IndexFile = Join-Path $ReportDir 'index.html'
if (-not (Test-Path $IndexFile)) {
  throw "ReportGenerator no genero index.html en: $ReportDir"
}

$GithubSummaryFile = Join-Path $ReportDir 'SummaryGithub.md'
if ((Test-Path $GithubSummaryFile) -and -not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
  Get-Content $GithubSummaryFile | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
}

Write-Host "Reporte generado en: $IndexFile"
