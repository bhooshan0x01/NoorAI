FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5158

# Install netcat and other required packages
RUN apt-get update && \
    apt-get install -y netcat-traditional postgresql-client curl && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["NoorAI.API/NoorAI.API.csproj", "NoorAI.API/"]
RUN dotnet restore "NoorAI.API/NoorAI.API.csproj"

# Copy the rest of the code
COPY . .

WORKDIR "/src/NoorAI.API"

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Build and publish
RUN dotnet build "NoorAI.API.csproj" -c Release -o /app/build
RUN dotnet publish "NoorAI.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app

# Install PostgreSQL client and curl
RUN apt-get update && \
    apt-get install -y postgresql-client curl && \
    rm -rf /var/lib/apt/lists/*

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Create directories for source and published output
RUN mkdir -p /app/src /app/publish

# Copy the source code and published output to separate directories
COPY --from=build /src /app/src
COPY --from=build /app/publish /app/publish

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5158
ENV ASPNETCORE_HTTP_PORT=5158
ENV ASPNETCORE_HTTPS_PORT=
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=noor_ai_db;Username=noorai_user;Password=N00r@!;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=100;Connection Idle Lifetime=60;Connection Pruning Interval=10;"

EXPOSE 5158

# Create and set permissions for the entrypoint script
RUN echo '#!/bin/bash\n\
echo "=== Starting initialization script ==="\n\
\n\
echo "Waiting for PostgreSQL to be ready..."\n\
until PGPASSWORD=N00r@! psql -h postgres -U noorai_user -d noor_ai_db -c "SELECT 1" > /dev/null 2>&1; do\n\
  echo "Waiting for PostgreSQL..."\n\
  sleep 2\n\
done\n\
echo "PostgreSQL connection successful!"\n\
\n\
echo "Applying database migrations..."\n\
cd /app/src/NoorAI.API\n\
\n\
# Retry migration up to 3 times\n\
for i in {1..3}; do\n\
  echo "Migration attempt $i of 3"\n\
  if dotnet ef database update; then\n\
    echo "Migrations applied successfully"\n\
    break\n\
  else\n\
    echo "Migration attempt $i failed"\n\
    if [ $i -eq 3 ]; then\n\
      echo "All migration attempts failed"\n\
      exit 1\n\
    fi\n\
    echo "Waiting before retry..."\n\
    sleep 5\n\
  fi\n\
done\n\
\n\
cd /app/publish\n\
\n\
echo "Starting the application..."\n\
exec dotnet NoorAI.API.dll' > /app/entrypoint.sh && \
chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]