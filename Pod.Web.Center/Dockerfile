# syntax=docker/dockerfile:1.7
#
# Multi-stage build for Pod.Web.Center.
#   stage 1: SDK image runs `dotnet publish` for the server project.
#   stage 2: ASP.NET runtime image carries only the published output.
#
# Build context is the repo root so we can reach the netstandard2.0 sibling
# projects Pod.Web.Center transitively references.
#
#   docker build -f Pod.Web.Center/Dockerfile -t pod-web-center .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore against just the projects in the dep graph to keep the layer small.
# Order: leaf netstandard2.0 libs first, then net10 libs, then the server.
COPY global.json ./
COPY Pod.Enums/Pod.Enums.csproj                                                       Pod.Enums/
COPY Pod.Data.Infrastructure/Pod.Data.Infrastructure.csproj                           Pod.Data.Infrastructure/
COPY Pod.Data.Models/Pod.Data.Models.csproj                                           Pod.Data.Models/
COPY Pod.DtoModels/Pod.DtoModels.csproj                                               Pod.DtoModels/
COPY Pod.ViewModels/Pod.ViewModels.csproj                                             Pod.ViewModels/
COPY Pod.ViewModels.Expressions/Pod.ViewModels.Expressions.csproj                     Pod.ViewModels.Expressions/
COPY Pod.MailEngine/Pod.MailEngine.csproj                                             Pod.MailEngine/
COPY Pod.Grpc.Const/Pod.Grpc.Base.Const.csproj                                        Pod.Grpc.Const/
COPY Pod.Grpc.Messages/Pod.Grpc.Messages.csproj                                       Pod.Grpc.Messages/
COPY Pod.Grpc.Utilities/Pod.Grpc.Utilities.csproj                                     Pod.Grpc.Utilities/
COPY Pod.Grpc.Base.Client/Pod.Grpc.Base.Client.csproj                                 Pod.Grpc.Base.Client/
COPY Pod.Grpc.Base/Pod.Grpc.Base.csproj                                               Pod.Grpc.Base/
COPY Pod.Data/Pod.Data.csproj                                                         Pod.Data/
COPY Pod.LetsEncrypt/Pod.LetsEncrypt.csproj                                           Pod.LetsEncrypt/
COPY Pod.Web.Authentication.ApiKeySecret/Pod.Web.Authentication.ApiKeySecret.csproj   Pod.Web.Authentication.ApiKeySecret/
COPY Pod.Services/Pod.Services.csproj                                                 Pod.Services/
COPY Pod.Grpc.Base.Server/Pod.Grpc.Base.Server.csproj                                 Pod.Grpc.Base.Server/
COPY Pod.Grpc.ConnectHost.Server/Pod.Grpc.ConnectHost.Server.csproj                   Pod.Grpc.ConnectHost.Server/
COPY Pod.Grpc.ShellHost.Server/Pod.Grpc.ShellHost.Server.csproj                       Pod.Grpc.ShellHost.Server/
COPY Pod.Web.Center/Pod.Web.Center.csproj                                             Pod.Web.Center/
RUN dotnet restore Pod.Web.Center/Pod.Web.Center.csproj

# Now copy the rest of the sources and publish.
COPY Pod.Enums/                            Pod.Enums/
COPY Pod.Data.Infrastructure/              Pod.Data.Infrastructure/
COPY Pod.Data.Models/                      Pod.Data.Models/
COPY Pod.DtoModels/                        Pod.DtoModels/
COPY Pod.ViewModels/                       Pod.ViewModels/
COPY Pod.ViewModels.Expressions/           Pod.ViewModels.Expressions/
COPY Pod.MailEngine/                       Pod.MailEngine/
COPY Pod.Grpc.Const/                       Pod.Grpc.Const/
COPY Pod.Grpc.Messages/                    Pod.Grpc.Messages/
COPY Pod.Grpc.Utilities/                   Pod.Grpc.Utilities/
COPY Pod.Grpc.Base.Client/                 Pod.Grpc.Base.Client/
COPY Pod.Grpc.Base/                        Pod.Grpc.Base/
COPY Pod.Data/                             Pod.Data/
COPY Pod.LetsEncrypt/                      Pod.LetsEncrypt/
COPY Pod.Web.Authentication.ApiKeySecret/  Pod.Web.Authentication.ApiKeySecret/
COPY Pod.Services/                         Pod.Services/
COPY Pod.Grpc.Base.Server/                 Pod.Grpc.Base.Server/
COPY Pod.Grpc.ConnectHost.Server/          Pod.Grpc.ConnectHost.Server/
COPY Pod.Grpc.ShellHost.Server/            Pod.Grpc.ShellHost.Server/
COPY Pod.Web.Center/                       Pod.Web.Center/
RUN dotnet publish Pod.Web.Center/Pod.Web.Center.csproj \
        -c Release \
        -o /app/publish \
        /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for the docker-compose HTTP healthcheck. The aspnet runtime image
# is debian-slim and doesn't ship curl or wget; without one the healthcheck just
# never resolves and the container stays in "starting" forever.
# Pre-create writable directories for the non-root app user. NLog writes to
# /app/logs/ (see nlog.config); without this the first log call fails with
# "Access to the path '/app/logs' is denied". /app/certs and /app/data are
# also pre-created so the named-volume mounts inherit the right owner.
USER root
RUN apt-get update \
        && apt-get install -y --no-install-recommends curl \
        && rm -rf /var/lib/apt/lists/* \
        && mkdir -p /app/logs /app/certs /app/data \
        && chown -R app:app /app

# Non-root user. The aspnet base image ships with the `app` user (UID 1654).
USER app

COPY --from=build --chown=app:app /app/publish .

# Persistent volumes:
#   /app/certs      - Let's Encrypt account key + issued chain
#   /app/data       - any future runtime state
VOLUME ["/app/certs", "/app/data"]

# Match the LetsEncryptOptions defaults (80 for ACME, 443 for REST+gRPC over TLS).
EXPOSE 80
EXPOSE 443

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true

ENTRYPOINT ["dotnet", "Pod.Web.Center.dll"]
