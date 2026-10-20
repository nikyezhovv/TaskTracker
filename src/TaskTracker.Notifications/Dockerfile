FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workspace

COPY ["src/TaskTracker.Notifications/TaskTracker.Notifications.csproj", "src/TaskTracker.Notifications/"]
COPY ["src/TaskTracker.Shared/TaskTracker.Shared.csproj", "src/TaskTracker.Shared/"]
RUN dotnet restore "src/TaskTracker.Notifications/TaskTracker.Notifications.csproj"

COPY ["src/TaskTracker.Notifications/", "src/TaskTracker.Notifications/"]
COPY ["src/TaskTracker.Shared/", "src/TaskTracker.Shared/"]
RUN dotnet publish "src/TaskTracker.Notifications/TaskTracker.Notifications.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskTracker.Notifications.dll"]