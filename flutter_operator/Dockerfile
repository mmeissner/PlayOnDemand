# syntax=docker/dockerfile:1.7
#
# Multi-stage build for the PoD operator frontend.
#   stage 1: install Flutter SDK + run `flutter build web` against this project.
#   stage 2: nginx serves the static build output and proxies /api/* + /swagger/*
#            to the upstream Pod.Web.Center server container.
#
# Build from the repo root:
#   docker build -f flutter_operator/Dockerfile -t pod-operator .

FROM ghcr.io/cirruslabs/flutter:3.27.0 AS build
# Pre-built Flutter Linux SDK image (Cirrus Labs - widely used in CI). Saves the
# ~1.5 GB SDK download and the apt-cert dance against archive.ubuntu.com from a
# debian/ubuntu base. flutter pub get + flutter build web work directly.
RUN git config --global --add safe.directory '*' && flutter config --enable-web

WORKDIR /src
COPY flutter_operator/pubspec.yaml flutter_operator/
WORKDIR /src/flutter_operator
RUN flutter pub get

COPY flutter_operator/lib  ./lib
COPY flutter_operator/web  ./web
RUN flutter build web --release

# -----------------------------------------------------------------------------
FROM nginx:1.27-alpine AS runtime
COPY --from=build /src/flutter_operator/build/web /usr/share/nginx/html
COPY flutter_operator/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
