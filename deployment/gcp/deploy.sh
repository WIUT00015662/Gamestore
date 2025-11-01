#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/opt/gamestore"
COMPOSE_FILE="docker-compose.prod.yml"
ENV_FILE="deployment/gcp/.env.prod"

cd "$APP_DIR"

echo "Pulling latest source..."
git pull origin main

echo "Pulling latest images from Docker Hub..."
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" pull

echo "Stopping old containers..."
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" down --remove-orphans

echo "Starting new containers..."
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --force-recreate

echo "Container status:"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps

echo "Cleaning old dangling images..."
docker image prune -af

echo "Deployment complete."
