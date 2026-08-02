FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

USER root
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R "$APP_UID:$APP_UID" /home/app/.aspnet
    
USER $APP_UID
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION=0.0.0-dev
ARG APP_INFORMATIONAL_VERSION=development
WORKDIR /src

COPY ["GeoBingo.Api/GeoBingo.Api.csproj", "GeoBingo.Api/"]
COPY ["GeoBingo.Contracts/GeoBingo.Contracts.csproj", "GeoBingo.Contracts/"]
COPY ["GeoBingo.Data/GeoBingo.Data.csproj", "GeoBingo.Data/"]
COPY ["GeoBingo.GameModes/GeoBingo.GameModes.csproj", "GeoBingo.GameModes/"]
COPY ["GeoBingo.Observability/GeoBingo.Observability.csproj", "GeoBingo.Observability/"]
COPY [".config/dotnet-tools.json", ".config/dotnet-tools.json"]

RUN dotnet restore "./GeoBingo.Api/GeoBingo.Api.csproj"
COPY . .
WORKDIR "/src/GeoBingo.Api"
RUN dotnet build "./GeoBingo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build \
    /p:Version=$APP_VERSION \
    /p:InformationalVersion=$APP_INFORMATIONAL_VERSION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG APP_VERSION
ARG APP_INFORMATIONAL_VERSION
RUN dotnet publish "./GeoBingo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false \
    /p:Version=$APP_VERSION \
    /p:InformationalVersion=$APP_INFORMATIONAL_VERSION

FROM build AS migrations-build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet tool restore
RUN dotnet ef migrations bundle \
    --project GeoBingo.Data/GeoBingo.Data.csproj \
    --startup-project GeoBingo.Api/GeoBingo.Api.csproj \
    --configuration $BUILD_CONFIGURATION \
    --output /app/migrations/efbundle

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeoBingo.Api.dll"]

FROM base AS migrations
COPY --from=migrations-build /app/migrations/efbundle .
ENTRYPOINT ["/bin/sh", "-c", "exec /app/efbundle --connection \"$ConnectionStrings__DefaultConnection\""]
