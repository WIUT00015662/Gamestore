#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/opt/gamestore"
COMPOSE_FILE="docker-compose.prod.yml"
ENV_FILE="deployment/gcp/.env.prod"

cd "$APP_DIR"

if [ ! -f "$ENV_FILE" ]; then
	echo "Missing $ENV_FILE on VM. Create it with production secrets before deploying."
	exit 1
fi

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
