#!/bin/bash

# Application user initialization script for SQL Server
echo "Starting application user initialization..."

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
until /opt/mssql-tools18/bin/sqlcmd -C -S ${DB_HOST:-localhost} -U ${DB_USER:-sa} -P ${DB_PASSWORD} -Q "SELECT 1" > /dev/null 2>&1
do
    echo "SQL Server is not ready yet, retrying in 2 seconds..."
    sleep 2
done

echo "SQL Server is ready!"

# Wait a bit more to ensure database is fully ready
sleep 5

# Execute all SQL scripts in order
for sql_file in /init-scripts/*.sql
do
    if [ -f "$sql_file" ]; then
        echo "Executing $sql_file..."
        /opt/mssql-tools18/bin/sqlcmd -C -S ${DB_HOST:-localhost} -U ${DB_USER:-sa} -P ${DB_PASSWORD} -i "$sql_file"
        
        if [ $? -eq 0 ]; then
            echo "Successfully executed $sql_file"
        else
            echo "Warning: Error executing $sql_file (this might be expected if EF migrations haven't run yet)"
        fi
    fi
done

echo "Application user initialization completed!"