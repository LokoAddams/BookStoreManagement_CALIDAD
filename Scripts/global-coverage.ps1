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
$ClassFilters = '-MicroServiceUsers.Application.DTOs.ChangePasswordDto;-MicroServiceUsers.Application.Services.UserService;-MicroServiceUsers.Domain.Models.PagedResult`1;-MicroServiceUsers.Domain.Models.Role;-MicroServiceUsers.Domain.Validations.ValidationException;-MicroServiceUsers.Infrastructure.DataBase.DataBaseConnection;-MicroServiceUsers.Infrastructure.Email.SendGridEmailService;-MicroServiceUsers.Infrastructure.Email.SendGridOptions;-MicroServiceUsers.Infrastructure.Repositories.RoleRepository;-MicroServiceUsers.Infrastructure.Repositories.UserRepository;-MicroServiceSales.Domain.Validations.ValidationException;-MicroServiceSales.Infrastructure.Repositories.SalesRepository;-MicroServiceSales.Infrastructure.DataBase.DataBaseConnection; -MicroServiceProduct.Infraestructure.DataBase.DataBaseConnection; -MicroServiceProduct.Infraestructure.Repository.CategoryRepository; -MicroServiceProduct.Infraestructure.Repository.ProductRepository'
reportgenerator `
  $ReportsArg `
  "-targetdir:$ReportDir" `
  "-assemblyfilters:+*;-*.Tests;-*UnitTest" `
    "-classfilters:$ClassFilters" `
  "-filefilters:+*;-*ValidationError.cs" `
  "-reporttypes:Html;MarkdownSummaryGithub"

$MicroserviceSummaries = @()
$Microservices = @(
  @{ Name = 'Sales'; AssemblyFilter = '+MicroServiceSales.*' },
  @{ Name = 'Client'; AssemblyFilter = '+MicroServiceClient.*' },
  @{ Name = 'Product'; AssemblyFilter = '+MicroServiceProduct.*' },
  @{ Name = 'Distributors'; AssemblyFilter = '+MicroServiceDistributors.*' },
  @{ Name = 'Users'; AssemblyFilter = '+MicroServiceUsers.*' },
  @{ Name = 'Reports'; AssemblyFilter = '+MicroServiceReports.*' },
  @{ Name = 'Web'; AssemblyFilter = '+MicroServiceWeb.*' }
)

foreach ($Microservice in $Microservices) {
  $MicroserviceReportDir = Join-Path $ReportDir $Microservice.Name
  New-Item -ItemType Directory -Path $MicroserviceReportDir -Force | Out-Null

  reportgenerator `
    $ReportsArg `
    "-targetdir:$MicroserviceReportDir" `
    "-assemblyfilters:$($Microservice.AssemblyFilter);-*.Tests;-*UnitTest" `
    "-classfilters:$ClassFilters" `
    "-filefilters:+*;-*ValidationError.cs" `
    "-reporttypes:Html;MarkdownSummaryGithub"

  $MicroserviceSummaryFile = Join-Path $MicroserviceReportDir 'SummaryGithub.md'
  $MicroserviceCoverage = 'N/A'

  if (Test-Path $MicroserviceSummaryFile) {
    $SummaryContent = Get-Content $MicroserviceSummaryFile -Raw
    $CoverageMatch = [regex]::Match($SummaryContent, '(\d+(?:\.\d+)?)\s*%')
    if ($CoverageMatch.Success) {
      $MicroserviceCoverage = $CoverageMatch.Groups[1].Value + '%'
    }
  }

  $MicroserviceSummaries += [pscustomobject]@{
    Name = $Microservice.Name
    Coverage = $MicroserviceCoverage
  }

  if ((Test-Path $MicroserviceSummaryFile) -and -not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value "`
## $($Microservice.Name)"
    Get-Content $MicroserviceSummaryFile | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value "`n"
  }
}

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
  Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value "`
## Cobertura por microservicio`
| Microservicio | Cobertura |
| --- | ---: |
"

  foreach ($MicroserviceSummary in $MicroserviceSummaries) {
    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value "| $($MicroserviceSummary.Name) | $($MicroserviceSummary.Coverage) |"
  }

  Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value "`n"
}

$IndexFile = Join-Path $ReportDir 'index.html'
if (-not (Test-Path $IndexFile)) {
  throw "ReportGenerator no genero index.html en: $ReportDir"
}

$GithubSummaryFile = Join-Path $ReportDir 'SummaryGithub.md'
if ((Test-Path $GithubSummaryFile) -and -not [string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY)) {
  Get-Content $GithubSummaryFile | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
}

Write-Host "Reporte generado en: $IndexFile"
