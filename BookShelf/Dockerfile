# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy solution and project files first for better layer caching
COPY ["BookShelf/BookShelf.csproj", "BookShelf/"]
COPY ["BookShelf.Application/BookShelf.Application.csproj", "BookShelf.Application/"]
COPY ["BookShelf.Domain/BookShelf.Domain.csproj", "BookShelf.Domain/"]
COPY ["BookShelf.Infrastructure/BookShelf.Infrastructure.csproj", "BookShelf.Infrastructure/"]

# Restore packages
RUN dotnet restore "BookShelf/BookShelf.csproj"

# Copy all source code
COPY . .
WORKDIR "/src/BookShelf"
RUN dotnet build "BookShelf.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookShelf.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookShelf.dll"]