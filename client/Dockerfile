# Multi-stage build for the Angular client.
#
# node stage: runs `ng build` to produce /app/dist/client/browser
# nginx stage: serves that dist folder + rewrites unknown paths to
# index.html so the SPA router can pick them up. Requests to /api/*
# are proxied to the API service (name resolves via docker-compose).

# --- build ---------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /app

# Lockfile copied first so npm ci is cached across source edits.
COPY client/package.json client/package-lock.json* ./
RUN npm ci --no-audit --no-fund

COPY client/. .
RUN npx ng build --configuration=production

# --- runtime -------------------------------------------------------------
FROM nginx:1.27-alpine AS runtime

# Config: SPA fallback + proxy /api/* to the api service. Kept in a
# sibling file so tweaks don't require editing this Dockerfile.
COPY client/nginx.conf /etc/nginx/conf.d/default.conf

# Angular 22 emits /dist/client/browser. Serve exactly that folder.
COPY --from=build /app/dist/client/browser /usr/share/nginx/html

EXPOSE 80
