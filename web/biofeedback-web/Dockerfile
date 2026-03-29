# 1. Stage: build React app (zůstává stejná)
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npx vite build

# 2. Stage: Nginx s automatickým generováním certifikátu
FROM nginx:alpine

# Instalace openssl - opravený příkaz
RUN apk add --no-cache openssl

# Odstranění defaultů
RUN rm -rf /usr/share/nginx/html/*
RUN rm /etc/nginx/conf.d/default.conf

# Příprava složky pro certifikáty
RUN mkdir -p /etc/nginx/ssl

# Zkopíruj tvůj nginx.conf
COPY nginx.conf /etc/nginx/conf.d/
COPY --from=build /app/dist /usr/share/nginx/html

# Příkaz pro vygenerování certifikátu a spuštění nginxu
# Používáme -nodes (no DES), aby to nevyžadovalo heslo při startu
CMD ["sh", "-c", "\
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /etc/nginx/ssl/nginx.key \
    -out /etc/nginx/ssl/nginx.crt \
    -subj '/C=CZ/ST=Prague/L=Prague/O=Biofeedback/CN=localhost'; \
    nginx -g 'daemon off;'"]

EXPOSE 80 443