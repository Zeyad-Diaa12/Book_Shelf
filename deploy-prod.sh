#!/bin/bash
# Production deployment script for BookShelf

# Load environment variables from .env.prod file
set -a
source .env.prod
set +a

# Pull latest changes if deploying from git
# git pull origin main

# Build and start the production containers
docker-compose -f docker-compose.prod.yml build --no-cache
docker-compose -f docker-compose.prod.yml up -d

# Display container status
echo "Container status:"
docker-compose -f docker-compose.prod.yml ps

# Display logs for the application container
echo "Application logs:"
docker-compose -f docker-compose.prod.yml logs --tail=100 bookshelf-app