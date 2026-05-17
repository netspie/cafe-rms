# syntax=docker/dockerfile:1

# --- build stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (separate layer for caching)
COPY CafeRMS.Api/CafeRMS.Api.csproj CafeRMS.Api/
RUN dotnet restore CafeRMS.Api/CafeRMS.Api.csproj

# Build + publish
COPY CafeRMS.Api/ CafeRMS.Api/
RUN dotnet publish CafeRMS.Api/CafeRMS.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# --- runtime stage -------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CafeRMS.Api.dll"]
