FROM envoyproxy/envoy-alpine:v1.16-latest

# Required for envsubst
RUN apk add gettext

WORKDIR /etc/envoy

COPY ./ApiProxy .

RUN chmod +x ./docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]

CMD ["envoy", "-c", "./envoy.yaml"]
