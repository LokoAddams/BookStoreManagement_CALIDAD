#!/bin/bash
set -e

databases=(
  "MicroServicioClient:tables/clients.sql"
  "MicroServicioDistributors:tables/distributors.sql"
  "MicroServicioProduct:tables/product.sql"
  "microservice_reports:tables/reports.sql"
  "microservicesales:tables/sales.sql"
  "users_services:tables/user.sql"
)

for db_info in "${databases[@]}"; do
  db_name="${db_info%%:*}"
  sql_file="${db_info#*:}"

  echo "--- Creando base de datos: $db_name ---"
  # Agregamos comillas dobles al nombre de la DB para que respete Mayúsculas
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -c "CREATE DATABASE \"$db_name\";"

  echo "--- Cargando tablas en $db_name desde $sql_file ---"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$db_name" -f "/docker-entrypoint-initdb.d/$sql_file"
done