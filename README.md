# Chatbot AI Service

## Environment Setup

### Required .env files

**For "production" deployment (docker-compose.deploy.yaml):**

```bash
cp .env.prod .env           # Root directory - Database configuration for Docker
cp .env.prod backend/.env   # Backend - API configuration  
cp .env.prod frontend/.env  # Frontend - Frontend configuration
```

**For local development:**

```bash
cp .env.dev .env            # Root directory - Database configuration for Docker
cp .env.dev backend/.env    # Backend - API configuration  
cp .env.dev frontend/.env   # Frontend - Frontend configuration
```

## Production Deployment

```bash
docker compose -f docker-compose.deploy.yaml up -d --build
```

## Local Development

### Database Setup

```bash
docker compose up -d  # Start database only
```

### Backend Setup

```bash
cd backend
# Configure backend/.env:
# - DB_HOST=localhost (not 'db')
# - CONNECTION_STRING with localhost
# - CORS_ORIGINS=http://localhost:4200
# - ASPNETCORE_ENVIRONMENT=Development
# - Set OPENAI_API_KEY, OPENAI_API_ENDPOINT, OPENAI_MODEL

dotnet ef database update  # Apply migrations
dotnet run                 # Start API
```

### Frontend Setup

```bash
cd frontend
npm install
npm run start
```

### Login Credentials

Use any of these demo emails to log in:

- `demo1@example.com`
- `demo2@example.com`  
- `test@example.com`
- `admin@example.com`

### Notes

- Use `dotnet ef database update` after schema changes
- Run `docker compose restart db-init` to refresh sample data
