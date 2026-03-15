#!/bin/sh
set -eu

db_mappings='
MicroServicioClient:tables/clients.sql
MicroServicioDistributors:tables/distributors.sql
MicroServicioProduct:tables/product.sql
microservice_reports:tables/reports.sql
microservicesales:tables/sales.sql
users_services:tables/user.sql
'

echo "$db_mappings" | while IFS=':' read -r db_name sql_file; do
  [ -z "$db_name" ] && continue

  echo "--- Verificando base de datos: $db_name ---"
  db_exists=$(psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -tAc "SELECT 1 FROM pg_database WHERE datname = '$db_name';")
  if [ "$db_exists" != "1" ]; then
    # Comillas dobles para preservar nombres con mayusculas y minusculas.
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" -c "CREATE DATABASE \"$db_name\";"
  fi

  echo "--- Cargando tablas en $db_name desde $sql_file ---"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$db_name" -f "/docker-entrypoint-initdb.d/$sql_file"
done