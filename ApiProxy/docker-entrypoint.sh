#!/usr/bin/env sh
set -eu

# Compile nginx conf with environment variable substitution
# Replace `${ENV_PLACEHOLDER}` with your environment variable.
# Interpolate multiple envs: `${ENV1},${ENV2}`
envsubst '${API_HOST},${API_PORT}' < /etc/envoy/envoy.template.yaml > /etc/envoy/envoy.yaml

# Reference: https://stackoverflow.com/questions/39082768/what-does-set-e-and-exec-do-for-docker-entrypoint-scripts
exec "$@"
