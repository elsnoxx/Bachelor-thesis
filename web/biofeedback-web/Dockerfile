# 1. Stage: build React app
FROM node:20 AS build

WORKDIR /app

# Zkopíruj package.json a package-lock.json
COPY package*.json ./

# Nainstaluj dependencies
RUN npm install

# Zkopíruj celý projekt
COPY . .

# Build pro produkci
# RUN npm run build

# Testing Build
RUN npx vite build

# 2. Stage: servírování přes Nginx
FROM nginx:alpine

# Odstran defaultní obsah
RUN rm -rf /usr/share/nginx/html/*
RUN rm /etc/nginx/conf.d/default.conf

# Zkopíruj vlastní Nginx config
COPY nginx.conf /etc/nginx/conf.d/

# Zkopíruj build React aplikace do Nginx
# COPY --from=build /app/build /usr/share/nginx/html

# Testing Copy
COPY --from=build /app/dist /usr/share/nginx/html


EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
