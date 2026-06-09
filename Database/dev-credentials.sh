#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres <<SQL
ALTER ROLE app_user   WITH PASSWORD '${APP_USER_PASSWORD}';
ALTER ROLE app_system WITH PASSWORD '${APP_SYSTEM_PASSWORD}';
SQL
