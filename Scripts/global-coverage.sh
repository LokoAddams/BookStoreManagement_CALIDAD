#!/usr/bin/env sh
set -eu

# Resuelve la raiz del repositorio a partir de la ubicacion del script.
SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/.." && pwd)
SOLUTION_PATH="$REPO_ROOT/Microservices.Orchestrator.sln"
SALES_TEST_PROJECT="$REPO_ROOT/MicroServiceSales/MicroServiceSales.Domain.UnitTest/MicroServiceSales.Domain.UnitTest.csproj"
SALES_INFRA_TEST_PROJECT="$REPO_ROOT/MicroServiceSales.Infrastructure.UnitTests/MicroServiceSales.Infrastructure.UnitTests.csproj"
RESULTS_DIR="$REPO_ROOT/TestResults"
REPORT_DIR="$REPO_ROOT/GlobalCoverageReport"

# Asegura que el binario global de .NET Tools quede disponible en esta sesion.
export PATH="$PATH:$HOME/.dotnet/tools"

if [ ! -f "$SOLUTION_PATH" ]; then
  echo "No se encontro la solucion en: $SOLUTION_PATH" >&2
  exit 1
fi

if [ ! -f "$SALES_TEST_PROJECT" ]; then
  echo "No se encontro el proyecto de pruebas de Sales en: $SALES_TEST_PROJECT" >&2
  exit 1
fi

if [ ! -f "$SALES_INFRA_TEST_PROJECT" ]; then
  echo "No se encontro el proyecto de pruebas de Infrastructure de Sales en: $SALES_INFRA_TEST_PROJECT" >&2
  exit 1
fi

# Genera una corrida limpia para evitar mezclar artefactos anteriores.
rm -rf "$RESULTS_DIR" "$REPORT_DIR"
mkdir -p "$RESULTS_DIR" "$REPORT_DIR"

# Instala el generador de reportes solo si aun no esta disponible.
if ! command -v reportgenerator >/dev/null 2>&1; then
  dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Ejecuta toda la solucion y centraliza los resultados de cobertura en la raiz.
dotnet test "$SOLUTION_PATH" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$RESULTS_DIR"

# Ejecuta explicitamente los tests de Sales para incluir su cobertura global.
dotnet test "$SALES_TEST_PROJECT" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$RESULTS_DIR"

# Ejecuta explicitamente los tests de infraestructura de Sales para incluir su cobertura global.
dotnet test "$SALES_INFRA_TEST_PROJECT" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$RESULTS_DIR"

# Fusiona todos los coverage.cobertura.xml generados por la solucion en un unico HTML.
reportgenerator \
  -reports:"$RESULTS_DIR/**/coverage.cobertura.xml" \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:Html

echo "Reporte generado en: $REPORT_DIR/index.html"