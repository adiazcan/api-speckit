# Deployment Guide

This guide covers deploying the IoT Robot Control & Telemetry API using Docker.

## Prerequisites

- Docker Engine 20.10+ installed
- Docker Compose 2.0+ (optional, for simplified deployment)
- Minimum 512MB RAM, 1GB disk space
- Network access for HTTP traffic (default port 5000)

## Quick Start with Docker Compose

The easiest way to deploy is using docker-compose:

```bash
# Clone the repository
git clone https://github.com/your-org/api-speckit.git
cd api-speckit

# Start the API
docker-compose up -d

# Check health
curl http://localhost:5000/v1/health
```

## Docker Deployment

### Building the Image

```bash
# Build the Docker image
docker build -t robot-api:1.0.0 .

# Verify the image
docker images | grep robot-api
```

### Running the Container

```bash
# Run with default configuration
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  robot-api:1.0.0

# Run with custom environment variables
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS="http://+:5000" \
  -e Cors__AllowedOrigins__0="https://your-frontend.com" \
  -v $(pwd)/logs:/app/logs \
  --restart unless-stopped \
  robot-api:1.0.0
```

### Viewing Logs

```bash
# View real-time logs
docker logs -f robot-api

# View last 100 lines
docker logs --tail 100 robot-api

# Check logs in mounted volume
tail -f logs/robot-api.log
```

### Health Checks

The API includes built-in health check endpoint at `/v1/health`:

```bash
# Check API health
curl http://localhost:5000/v1/health

# Response:
# {
#   "status": "healthy",
#   "timestamp": "2024-01-15T10:30:00Z",
#   "version": "1.0.0",
#   "service": "IoT Robot Control & Telemetry API"
# }
```

Docker automatically monitors health every 30 seconds:

```bash
# Check container health status
docker ps --filter name=robot-api --format "{{.Status}}"
```

## Environment Variables

Configure the API using environment variables:

### Core Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Development, Staging, Production) | `Production` |
| `ASPNETCORE_URLS` | URLs the API listens on | `http://+:5000` |

### CORS Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `Cors__AllowedOrigins__0` | First allowed CORS origin | `http://localhost:3000` |
| `Cors__AllowedOrigins__1` | Second allowed CORS origin | `http://localhost:8080` |

Add more origins by incrementing the index: `__2`, `__3`, etc.

### Logging Configuration

Logging is configured via `appsettings.json`. To override log level:

```bash
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  -e Serilog__MinimumLevel__Default=Information \
  -e Serilog__MinimumLevel__Override__Microsoft=Warning \
  robot-api:1.0.0
```

## Port Configuration

The API listens on port 5000 by default. To use a different port:

```bash
# Map to port 8080 on host
docker run -d \
  --name robot-api \
  -p 8080:5000 \
  robot-api:1.0.0

# Access at http://localhost:8080
```

To change the internal port:

```bash
docker run -d \
  --name robot-api \
  -p 8080:8080 \
  -e ASPNETCORE_URLS="http://+:8080" \
  --health-cmd="curl --fail http://localhost:8080/v1/health || exit 1" \
  robot-api:1.0.0
```

## Volume Mounts

### Log Files

Mount a volume to persist logs:

```bash
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  -v /var/log/robot-api:/app/logs \
  robot-api:1.0.0
```

### Configuration Files

Mount custom configuration:

```bash
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  -v $(pwd)/appsettings.Production.json:/app/appsettings.Production.json:ro \
  robot-api:1.0.0
```

## Docker Compose Configuration

The included `docker-compose.yml` provides a complete setup:

```yaml
version: '3.8'

services:
  robot-api:
    build: .
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - Cors__AllowedOrigins__0=http://localhost:3000
      - Cors__AllowedOrigins__1=http://localhost:8080
    volumes:
      - ./logs:/app/logs
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/v1/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s
    restart: unless-stopped
    networks:
      - robot-api-network

networks:
  robot-api-network:
    driver: bridge
```

### Commands

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose stop

# Remove services and volumes
docker-compose down -v

# Rebuild and restart
docker-compose up -d --build
```

## Production Considerations

### Security

1. **HTTPS**: Use a reverse proxy (nginx, Traefik) for TLS termination
2. **Authentication**: Replace mock authentication with real JWT validation
3. **Secrets**: Use Docker secrets or environment variable injection from a secret manager
4. **Network**: Use Docker networks to isolate the API from other services

Example nginx reverse proxy:

```nginx
server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;
    
    ssl_certificate /etc/ssl/certs/api.crt;
    ssl_certificate_key /etc/ssl/private/api.key;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Scaling

For horizontal scaling, use Docker Swarm or Kubernetes:

```bash
# Docker Swarm example
docker service create \
  --name robot-api \
  --replicas 3 \
  --publish 5000:5000 \
  robot-api:1.0.0
```

### Monitoring

1. **Health checks**: Configure orchestrator to use `/v1/health`
2. **Logs**: Forward logs to centralized logging (ELK, Splunk)
3. **Metrics**: Export metrics to Prometheus/Grafana
4. **Tracing**: Add distributed tracing with OpenTelemetry

### Persistence

The current implementation uses in-memory stores. For production:

1. Add a persistent database (PostgreSQL, MongoDB)
2. Mount data volumes for persistent storage
3. Configure database connection strings via environment variables

### Resource Limits

Set resource limits for stability:

```bash
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  --memory="512m" \
  --cpus="1.0" \
  robot-api:1.0.0
```

Or in docker-compose.yml:

```yaml
services:
  robot-api:
    # ...
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
```

## Troubleshooting

### Container won't start

```bash
# Check container logs
docker logs robot-api

# Check container status
docker ps -a | grep robot-api

# Inspect container
docker inspect robot-api
```

### Health check failing

```bash
# Test health endpoint manually
docker exec robot-api curl -f http://localhost:5000/v1/health

# Check if process is running
docker exec robot-api ps aux

# Check network connectivity
docker exec robot-api netstat -tlnp
```

### Permission issues

```bash
# The container runs as non-root user (appuser:1000)
# Ensure mounted volumes have correct permissions
sudo chown -R 1000:1000 ./logs
```

### Port already in use

```bash
# Find process using port 5000
sudo lsof -i :5000

# Use a different port
docker run -d -p 5001:5000 robot-api:1.0.0
```

## Updating the Deployment

```bash
# Pull latest code
git pull

# Rebuild image
docker build -t robot-api:1.0.1 .

# Stop old container
docker stop robot-api
docker rm robot-api

# Start new container
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  robot-api:1.0.1

# Or with docker-compose
docker-compose up -d --build
```

## Rollback

```bash
# Stop current version
docker stop robot-api
docker rm robot-api

# Start previous version
docker run -d \
  --name robot-api \
  -p 5000:5000 \
  robot-api:1.0.0
```

## Support

For issues or questions:

- Check logs: `docker logs robot-api`
- Verify health: `curl http://localhost:5000/v1/health`
- Review API documentation: `curl http://localhost:5000/v1/openapi.json`
- Consult API usage guide: `docs/API_USAGE.md`
