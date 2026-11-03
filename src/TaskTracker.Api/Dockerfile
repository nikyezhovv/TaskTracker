FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workspace

COPY ["Directory.Build.props", "./"]
COPY [".editorconfig", "./"]
COPY ["src/TaskTracker.Api/TaskTracker.Api.csproj", "src/TaskTracker.Api/"]
COPY ["src/TaskTracker.Shared/TaskTracker.Shared.csproj", "src/TaskTracker.Shared/"]
RUN dotnet restore "src/TaskTracker.Api/TaskTracker.Api.csproj"

COPY ["src/TaskTracker.Api/", "src/TaskTracker.Api/"]
COPY ["src/TaskTracker.Shared/", "src/TaskTracker.Shared/"]
RUN dotnet publish "src/TaskTracker.Api/TaskTracker.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]
