# TaskTracker

## Локальный запуск

В Windows запусти `start-all.cmd` из корня репозитория. Скрипт пересобирает и запускает сервисы через Docker Compose, ждёт готовности контейнеров и показывает логи.

Перед запуском скопируй `.env.example` в `.env` и задай собственный `JWT_SIGNING_KEY` длиной не менее 32 байт. Один и тот же ключ используется API для выдачи токенов и Notifications для проверки SignalR-подключения.

- Swagger UI: <http://localhost:8080/swagger>
- API readiness: <http://localhost:8080/health/ready>

API автоматически применяет EF Core migrations при старте. PostgreSQL создаёт базу `tasktracker`; миграция создаёт таблицы в пустой базе. Для существующей полной схемы из `schema.sql` API однократно добавит запись baseline в `__EFMigrationsHistory`, сохранив данные. `docker compose down` сохраняет volume базы; чтобы удалить данные вручную, используй `docker compose down --volumes`.

## Очередь напоминаний

RabbitMQ запускается вместе с сервисами. Панель управления доступна на <http://localhost:15672>; порт и пароль задаются через `.env` (`RABBITMQ_PORT`, `RABBITMQ_MANAGEMENT_PORT`, `RABBITMQ_PASSWORD`). Worker и Notifications подключаются к брокеру по внутреннему адресу `rabbitmq`.

Worker подключается к PostgreSQL и раз в 10 секунд выбирает незавершённые задачи с дедлайном в ближайшие 15 минут. EF Core-репозиторий в Shared выбирает кандидатов индексированным LINQ-запросом и одним условным `ExecuteUpdate` закрепляет пачку lease-токеном. При конкуренции реплик обновить строки сможет только одна; Worker публикует `TaskDeadlineReminderEvent`, затем помечает напоминание отправленным. Изменение дедлайна в API сбрасывает отметку.

Worker использует `ConnectionStrings__DefaultConnection`. В Compose ему передаётся та же база, что и API; в рабочей среде для Worker следует выдать отдельного пользователя PostgreSQL с доступом только к нужным столбцам таблицы `tasks`. Новые столбцы и индекс добавляются EF Core миграцией при старте API.

MassTransit создаёт для consumer-а Notifications отдельную очередь с retry; после исчерпания повторов сообщения попадают в error queue (`<имя-очереди>_error`). Notifications отправляет событие SignalR-клиенту с именем `ReceiveNotification` по адресу `/hubs/notifications`. Сейчас хаб принимает подключения без авторизации; для адресной доставки идентификатор пользователя должен быть доступен SignalR-соединению.

Worker помечает напоминание отправленным после публикации события в RabbitMQ. SignalR доставляет его только активным соединениям пользователя; если клиент не подключён к моменту обработки сообщения, браузер его не получит. Для уведомлений при закрытом сайте нужен Web Push.

Интеграционный тест очереди использует Testcontainers и требует запущенный Docker: `dotnet test tests/TaskTracker.IntegrationTests/TaskTracker.IntegrationTests.csproj`.
