FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /workspace
COPY ["src/TaskTracker.Notifications/TaskTracker.Notifications.csproj", "src/TaskTracker.Notifications/"]
COPY ["src/TaskTracker.Shared/TaskTracker.Shared.csproj", "src/TaskTracker.Shared/"]
RUN dotnet restore "src/TaskTracker.Notifications/TaskTracker.Notifications.csproj"
COPY . .
WORKDIR "/workspace/src/TaskTracker.Notifications"
RUN dotnet build "TaskTracker.Notifications.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskTracker.Notifications.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskTracker.Notifications.dll"]
