# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS client
WORKDIR /source
COPY Gwire/Gwire.csproj Gwire/
RUN dotnet restore Gwire/Gwire.csproj
COPY Gwire/ Gwire/
RUN dotnet publish Gwire/Gwire.csproj --configuration Release --no-restore --output /client \
    && mkdir /assets && cp -aL /client/wwwroot /assets/wwwroot

FROM debian:bookworm-slim AS icons
RUN apt-get update \
    && apt-get install -y --no-install-recommends librsvg2-bin \
    && rm -rf /var/lib/apt/lists/*
COPY Gwire/wwwroot/gwire-webicon.svg /favicon.svg
RUN rsvg-convert --width 1024 --height 1024 --output /icon.png /favicon.svg

FROM node:22-bookworm-slim AS macos-package
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /build
ENV ELECTRON_SKIP_BINARY_DOWNLOAD=1
COPY Desktop/package.json Desktop/main.cjs ./
RUN npm install --ignore-scripts
COPY --from=client /assets/wwwroot ./client/wwwroot
COPY --from=icons /icon.png ./build/icon.png
ARG APP_ARCH=arm64
ARG APP_VERSION
# Signing is performed on macOS after the app is exported from Docker.
RUN if [ -n "$APP_VERSION" ]; then npm pkg set "version=$APP_VERSION"; fi \
    && ./node_modules/.bin/electron-builder --mac --dir --${APP_ARCH} --publish never \
    && mkdir /output && mv dist/mac*/Gwire.app /output/Gwire.app

FROM scratch AS macos
COPY --from=macos-package /output/ /

FROM electronuserland/builder:wine AS windows-package
WORKDIR /build
ENV ELECTRON_SKIP_BINARY_DOWNLOAD=1
COPY Desktop/package.json Desktop/main.cjs ./
RUN npm install --ignore-scripts
COPY --from=client /assets/wwwroot ./client/wwwroot
COPY --from=icons /icon.png ./build/icon.png
ARG APP_ARCH=x64
ARG APP_VERSION
RUN if [ -n "$APP_VERSION" ]; then npm pkg set "version=$APP_VERSION"; fi \
    && ./node_modules/.bin/electron-builder --win portable --${APP_ARCH} --publish never \
    && mkdir /output \
    && mv dist/*.exe /output/ \
    && mv dist/win*-unpacked /output/win-unpacked

FROM scratch AS windows
COPY --from=windows-package /output/ /
