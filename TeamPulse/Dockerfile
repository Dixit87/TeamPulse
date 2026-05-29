# 1. SDK Image use karenge build ke liye
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Saara code pehle hi copy kar lenge taaki koi path ka issue na rahe
COPY . .
RUN dotnet restore

# Project ko release mode me build aur publish karenge
RUN dotnet publish -c Release -o /app/out

# 2. Runtime image taiyar karenge final run ke liye
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeamPulse.dll"]