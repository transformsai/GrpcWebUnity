# Stage: Build site
FROM node AS build

WORKDIR /usr/src

COPY ./Proto ./Proto
COPY ./Client ./app

WORKDIR /usr/src/app

RUN npm install

RUN npm run build

# Stage: Nginx config
FROM nginx:alpine

COPY ./Client/nginx.template.conf /tmp/nginx.template.conf

COPY ./Client/entrypoint.sh ./entrypoint.sh
RUN ["chmod", "+x", "/entrypoint.sh"]

COPY --from=build /usr/src/app/dist/ /data/www

# Compile nginx default.confg.template with environment variables
ENTRYPOINT ["/entrypoint.sh"]

CMD ["nginx", "-g", "daemon off;"]
