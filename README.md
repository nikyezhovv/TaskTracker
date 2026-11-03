# TaskTracker

## Локальный запуск

В Windows запусти `start-all.cmd` из корня репозитория. Скрипт пересобирает и запускает сервисы через Docker Compose, ждёт готовности контейнеров и показывает логи.

- Swagger UI: <http://localhost:8080/swagger>
- API readiness: <http://localhost:8080/health/ready>

API автоматически применяет EF Core migrations при старте. PostgreSQL создаёт базу `tasktracker`; миграция создаёт таблицы в пустой базе. Для существующей полной схемы из `schema.sql` API однократно добавит запись baseline в `__EFMigrationsHistory`, сохранив данные. `docker compose down` сохраняет volume базы; чтобы удалить данные вручную, используй `docker compose down --volumes`.

## Очередь напоминаний

RabbitMQ запускается вместе с сервисами. Панель управления доступна на <http://localhost:15672>; порт и пароль задаются через `.env` (`RABBITMQ_PORT`, `RABBITMQ_MANAGEMENT_PORT`, `RABBITMQ_PASSWORD`). Worker и Notifications подключаются к брокеру по внутреннему адресу `rabbitmq`.

Worker и Notifications подключаются к RabbitMQ через MassTransit. Контракт `TaskDeadlineReminderEvent` задаёт формат события напоминания; фоновая выборка дедлайнов и публикация события Worker-ом будут следующим этапом. MassTransit создаёт для consumer-а Notifications отдельную очередь с retry; сообщения после исчерпания повторов попадают в error queue MassTransit (`<имя-очереди>_error`). Notifications отправляет событие SignalR-клиенту с именем `ReceiveNotification` по адресу `/hubs/notifications`, используя идентификатор владельца из сообщения.

Интеграционный тест очереди использует Testcontainers и требует запущенный Docker: `dotnet test tests/TaskTracker.IntegrationTests/TaskTracker.IntegrationTests.csproj`.
