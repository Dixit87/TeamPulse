# 1. Build ke liye .NET 10.0 SDK use karenge
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Saari files ko copy karenge
COPY . .

# TeamPulse project ko target karke restore karenge
RUN dotnet restore "TeamPulse/TeamPulse.csproj"

# Project ko Release mode me publish karenge
RUN dotnet publish "TeamPulse/TeamPulse.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime ke liye .NET 10.0 ASP.NET image use karenge
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeamPulse.dll"]