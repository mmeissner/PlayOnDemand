# syntax=docker/dockerfile:1.7
#
# Multi-stage build for the PoD operator mobile/daily-ops app.
#   stage 1: install Flutter SDK + run `flutter build web` against this project.
#   stage 2: nginx serves the static build output and proxies /api/* to the
#            upstream Pod.Web.Center server container.
#
# Build from the repo root:
#   docker build -f flutter_operator_mobile/Dockerfile -t pod-operator-mobile .

FROM ghcr.io/cirruslabs/flutter:3.27.0 AS build
# Pre-built Flutter Linux SDK image (Cirrus Labs, widely used in CI). Same
# base as flutter_operator/Dockerfile so we share its layer cache.
RUN git config --global --add safe.directory '*' && flutter config --enable-web

WORKDIR /src
COPY flutter_operator_mobile/pubspec.yaml flutter_operator_mobile/
WORKDIR /src/flutter_operator_mobile
RUN flutter pub get

COPY flutter_operator_mobile/lib    ./lib
COPY flutter_operator_mobile/web    ./web
COPY flutter_operator_mobile/assets ./assets
COPY flutter_operator_mobile/analysis_options.yaml ./analysis_options.yaml
RUN flutter build web --release

# -----------------------------------------------------------------------------
FROM nginx:1.27-alpine AS runtime
COPY --from=build /src/flutter_operator_mobile/build/web /usr/share/nginx/html
COPY flutter_operator_mobile/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
