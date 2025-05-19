#!/bin/bash

# Set maximum retry attempts
MAX_RETRIES=30
RETRY_COUNT=0

echo "=== Starting initialization script ==="
echo "Current directory: $(pwd)"
echo "Listing files in current directory:"
ls -la

# Ensure PostgreSQL client is installed
if ! command -v psql &> /dev/null; then
    echo "Installing PostgreSQL client..."
    apt-get update && apt-get install -y postgresql-client
fi

# Ensure dotnet-ef is in PATH
export PATH="$PATH:/root/.dotnet/tools"
echo "Checking for dotnet-ef..."
which dotnet-ef || echo "dotnet-ef not found in PATH"

# Wait for PostgreSQL to be ready with timeout
echo "Waiting for PostgreSQL to be ready..."
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    echo "Attempt $((RETRY_COUNT+1))/$MAX_RETRIES to connect to PostgreSQL..."
    
    # Try to connect to PostgreSQL
    if PGPASSWORD=N00r@! psql -h postgres -U noorai_user -d noor_ai_db -c "SELECT 1" > /dev/null 2>&1; then
        echo "PostgreSQL connection successful!"
        break
    else
        echo "Connection attempt failed."
        RETRY_COUNT=$((RETRY_COUNT+1))
        sleep 5
    fi
done

# If connection failed after all retries
if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
    echo "ERROR: Failed to connect to PostgreSQL after $MAX_RETRIES attempts"
    exit 1
fi

# Try to run migrations
echo "Applying database migrations..."
if command -v dotnet-ef &> /dev/null; then
    echo "Using dotnet-ef to apply migrations"
    dotnet-ef database update --connection "${ConnectionStrings__DefaultConnection}"
    MIGRATION_RESULT=$?
    
    if [ $MIGRATION_RESULT -ne 0 ]; then
        echo "WARNING: Database migration using dotnet-ef failed with exit code $MIGRATION_RESULT"
        echo "This might be expected if migrations are not set up or already applied."
        echo "Continuing with application startup..."
    else
        echo "Database migrations applied successfully!"
    fi
else
    echo "WARNING: dotnet-ef tool not found, skipping migrations"
fi

# Start the application
echo "Starting the application..."
exec dotnet NoorAI.API.dll