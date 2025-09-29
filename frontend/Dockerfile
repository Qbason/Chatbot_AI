FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .
COPY ./.env .env

# Inject environment variables into environment.ts
RUN node inject-env.js

RUN npm run build

FROM nginx:alpine AS production

COPY --from=build /app/dist/ChatbotAI/browser /usr/share/nginx/html
COPY ./nginx.conf /etc/nginx/conf.d/default.conf

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

RUN chown -R appuser:appgroup /usr/share/nginx/html
RUN chown -R appuser:appgroup /var/cache/nginx
RUN chown -R appuser:appgroup /var/log/nginx
RUN chown -R appuser:appgroup /etc/nginx/conf.d
RUN touch /var/run/nginx.pid
RUN chown -R appuser:appgroup /var/run/nginx.pid

USER appuser

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
