# NoorAI Interview Platform

NoorAI is an AI-powered interview platform that conducts automated interviews based on your resume and job description.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 or later)
- [PostgreSQL](https://www.postgresql.org/download/) (v15 or later)
- [Docker](https://www.docker.com/get-started) and [Docker Compose](https://docs.docker.com/compose/install/) (for Docker setup)
- [Ollama](https://ollama.ai/) (for running the AI model)

## Running with Docker (Recommended)

### Option 1: Full Stack with Docker Compose

1. Clone the repository:

   ```
   git clone <repository-url>
   cd NoorAI
   ```

2. Start all services:

   ```
   docker compose up --build
   ```

   This will start:

   - PostgreSQL database
   - Ollama service
   - .NET API
   - Next.js UI

3. Access the application:
   - UI: http://localhost:8453
   - API: http://localhost:5158/swagger/index.html
   - PostgreSQL: localhost:5438
   - Ollama: localhost:11434

### Option 2: Manual Setup with Docker for Database

1. Start only the database:

   ```
   docker compose up postgres
   ```

2. Run the API locally:

   ```
   cd NoorAI.API
   dotnet restore
   dotnet run
   ```

3. Run the UI locally:

   ```
   cd noorai-ui
   npm install
   npm run dev
   ```

4. Start Ollama locally:
   ```
   ollama serve
   ollama pull deepseek-r1:8b
   ```

## Environment Variables

### API (.NET)

- `ASPNETCORE_ENVIRONMENT`: Production/Development
- `ASPNETCORE_URLS`: http://+:5158
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Ollama__BaseUrl`: http://localhost:11434 (or http://ollama:11434 for Docker)
- `Ollama__ModelName`: deepseek-r1:8b

### UI (Next.js)

- `NEXT_PUBLIC_API_URL`: API endpoint URL

## Troubleshooting

### Database Connection Issues

- Ensure PostgreSQL is running and accessible
- Check connection string in `appsettings.json`
- Verify database user permissions

### Ollama Connection Issues

- Ensure Ollama service is running
- Verify the model is downloaded: `ollama list`
- Check Ollama URL configuration
- If using Docker, pull the model manually: `docker exec noorai_ollama ollama pull deepseek-r1:8b`

### Docker Issues

- Check container logs: `docker compose logs`
- Ensure all ports are available
- Verify Docker network: `docker network ls`
- If Ollama model is missing, pull it manually using the command above
