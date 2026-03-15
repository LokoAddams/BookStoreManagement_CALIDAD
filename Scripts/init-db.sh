#!/bin/sh
set -eu

db_mappings='
MicroServicioClient:tables/clients.sql:inserts/insertCliente.sql
MicroServicioDistributors:tables/distributors.sql:inserts/insertDistributors.sql
MicroServicioProduct:tables/product.sql:inserts/insertProduct.sql
microservice_reports:tables/reports.sql:inserts/insertReports.sql
microservicesales:tables/sales.sql:inserts/insertSales.sql
users_services:tables/user.sql:inserts/insertUser.sql
'

echo "$db_mappings" | while IFS=':' read -r db_name schema_file seed_file; do
  [ -z "$db_name" ] && continue

  # Limpia fin de linea CRLF si el archivo fue editado en Windows.
  db_name=$(printf '%s' "$db_name" | tr -d '\r')
  schema_file=$(printf '%s' "$schema_file" | tr -d '\r')
  seed_file=$(printf '%s' "${seed_file:-}" | tr -d '\r')

  echo "--- Verificando base de datos: $db_name ---"
  db_exists=$(psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -tAc "SELECT 1 FROM pg_database WHERE datname = '$db_name';")
  if [ "$db_exists" = "1" ]; then
    echo "--- $db_name ya existe, se omite inicializacion de esquema y datos ---"
    continue
  fi

  if [ "$db_exists" != "1" ]; then
    # Comillas dobles para preservar nombres con mayusculas y minusculas.
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -c "CREATE DATABASE \"$db_name\";"
  fi

  echo "--- Cargando tablas en $db_name desde $schema_file ---"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$db_name" -f "/docker-entrypoint-initdb.d/$schema_file"

  if [ -n "$seed_file" ] && [ -f "/docker-entrypoint-initdb.d/$seed_file" ]; then
    echo "--- Cargando datos de ejemplo en $db_name desde $seed_file ---"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$db_name" -f "/docker-entrypoint-initdb.d/$seed_file"
  else
    echo "--- No se encontro archivo de datos para $db_name ($seed_file), se omite ---"
  fi
done