# TaskTracker

## Локальный запуск

В Windows запусти `start-all.cmd` из корня репозитория. Скрипт пересобирает и запускает сервисы через Docker Compose, ждёт готовности контейнеров и показывает логи.

- Swagger UI: <http://localhost:8080/swagger>
- API readiness: <http://localhost:8080/health/ready>

API автоматически применяет EF Core migrations при старте. PostgreSQL создаёт базу `tasktracker`; миграция создаёт таблицы в пустой базе. Для существующей полной схемы из `schema.sql` API однократно добавит запись baseline в `__EFMigrationsHistory`, сохранив данные. `docker compose down` сохраняет volume базы; чтобы удалить данные вручную, используй `docker compose down --volumes`.
