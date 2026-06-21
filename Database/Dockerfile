FROM postgres:18

COPY tkt.sql /docker-entrypoint-initdb.d/01-schema.sql
COPY --chmod=0755 dev-credentials.sh /docker-entrypoint-initdb.d/02-dev-credentials.sh
COPY seed.sql /docker-entrypoint-initdb.d/03-seed.sql
