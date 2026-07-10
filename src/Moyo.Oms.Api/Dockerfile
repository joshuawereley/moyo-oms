FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore before copying sources: Docker caches this layer, so restore only
# re-runs when a csproj or the central package list changes, not on every edit.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Moyo.Oms.Domain/Moyo.Oms.Domain.csproj                 src/Moyo.Oms.Domain/
COPY src/Moyo.Oms.Application/Moyo.Oms.Application.csproj       src/Moyo.Oms.Application/
COPY src/Moyo.Oms.Infrastructure/Moyo.Oms.Infrastructure.csproj src/Moyo.Oms.Infrastructure/
COPY src/Moyo.Oms.Api/Moyo.Oms.Api.csproj                       src/Moyo.Oms.Api/
RUN dotnet restore src/Moyo.Oms.Api/Moyo.Oms.Api.csproj

COPY src/ src/
RUN dotnet publish src/Moyo.Oms.Api/Moyo.Oms.Api.csproj -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# $APP_UID is a non-root user baked into the .NET 8 base images.
USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "Moyo.Oms.Api.dll"]
