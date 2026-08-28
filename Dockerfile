# Wixpack by Blitz — multi-stage build for Render
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Wixpack.Host/Wixpack.Host.csproj -c Release -o /app/publish \
    && mkdir -p /app/publish/config \
    && cp config/settings.json /app/publish/config/settings.json || true

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "Wixpack.Host.dll"]
