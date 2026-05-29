# 1. SDK Image use karenge build ke liye
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Project file copy karke restore karenge
COPY ["TeamPulse.csproj", "./"]
RUN dotnet restore "./TeamPulse.csproj"

# Baaki saara code copy karke build karenge
COPY . .
RUN dotnet build "TeamPulse.csproj" -c Release -o /app/build

# Publish karenge
FROM build AS publish
RUN dotnet publish "TeamPulse.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime image taiyar karenge final run ke liye
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeamPulse.dll"]