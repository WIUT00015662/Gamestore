# GCP VM Deployment Guide

## 1) VM prerequisites

Run on VM:

- Install Docker + Compose plugin
- Install Git
- Clone repo to `/opt/gamestore`

## 2) Configure environment

```bash
cd /opt/gamestore
cp deployment/gcp/.env.prod.example deployment/gcp/.env.prod
# edit deployment/gcp/.env.prod with production values
```

Your domain defaults in sample:
- Frontend: `experimentalapp.app`
- API: `api.experimentalapp.app`

## 3) Manual deploy (same logic used in CI)

```bash
cd /opt/gamestore
chmod +x deployment/gcp/deploy.sh
./deployment/gcp/deploy.sh
```

This will:
- pull latest code
- pull latest docker images from Docker Hub
- stop old containers and remove orphans
- start new containers with force recreate
- print container status
- remove old dangling images

## 4) HTTPS with Certbot (host-level Nginx)

```bash
cd /opt/gamestore
chmod +x deployment/gcp/setup-https-certbot.sh
./deployment/gcp/setup-https-certbot.sh experimentalapp.app api.experimentalapp.app your-email@example.com
```

## 5) GitHub Actions configuration (Workload Identity Federation)

Set these in repository settings:

### Secrets
- `DOCKERHUB_USERNAME` (use `n1dovud`)
- `DOCKERHUB_TOKEN`

### Variables
- `VITE_API_BASE_URL` (set to `https://api.experimentalapp.app`)
- `GCP_PROJECT_ID`
- `GCP_PROJECT_NUMBER`
- `GCP_POOL_ID`
- `GCP_PROVIDER_ID`
- `GCP_SERVICE_ACCOUNT`
- `GCP_VM_NAME`
- `GCP_ZONE`

## 6) IAM needed for GitHub OIDC service account

Grant the service account (used in WIF) permissions for deployment:
- `roles/compute.viewer`
- `roles/compute.instanceAdmin.v1`
- `roles/iam.serviceAccountUser`
- `roles/iam.serviceAccountTokenCreator` (if required by org policy)

For OS Login access over `gcloud compute ssh`, make sure it has one of:
- `roles/compute.osAdminLogin` (preferred)
- or `roles/compute.osLogin`

## Notes

- Pushing `:latest` replaces the previous latest pointer; explicit Docker Hub deletion is not required.
- Keep `deployment/gcp/.env.prod` only on VM (do not commit it).
- If deployment ever looks stale, run on VM:

```bash
cd /opt/gamestore
docker compose --env-file deployment/gcp/.env.prod -f docker-compose.prod.yml pull
docker compose --env-file deployment/gcp/.env.prod -f docker-compose.prod.yml down --remove-orphans
docker compose --env-file deployment/gcp/.env.prod -f docker-compose.prod.yml up -d --force-recreate
docker compose --env-file deployment/gcp/.env.prod -f docker-compose.prod.yml ps
