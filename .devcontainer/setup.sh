export PGPASSWORD='postgres'
psql -U postgres -h db -c "CREATE DATABASE gv;"
psql -U postgres -h db -d gv -a -f  create_tables.sql
psql -U postgres -h db -d gv -a -f  data_seed.sql