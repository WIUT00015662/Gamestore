#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -lt 3 ]; then
  echo "Usage: $0 <frontend_domain> <api_domain> <email>"
  exit 1
fi

FRONTEND_DOMAIN="$1"
API_DOMAIN="$2"
EMAIL="$3"

sudo apt-get update
sudo apt-get install -y nginx certbot python3-certbot-nginx

sudo tee /etc/nginx/sites-available/gamestore >/dev/null <<EOF
server {
    listen 80;
    server_name ${FRONTEND_DOMAIN};

    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}

server {
    listen 80;
    server_name ${API_DOMAIN};

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/gamestore /etc/nginx/sites-enabled/gamestore
sudo nginx -t
sudo systemctl reload nginx

sudo certbot --nginx -d "$FRONTEND_DOMAIN" -d "$API_DOMAIN" --non-interactive --agree-tos -m "$EMAIL" --redirect
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

echo "HTTPS setup complete."
