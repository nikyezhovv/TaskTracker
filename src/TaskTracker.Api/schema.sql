-- Черновая схема PostgreSQL для TaskTracker.
-- Перед применением подтвердите допущения из database-schema.md.
-- DDL рассчитан на PostgreSQL 13+.

BEGIN;

CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(320) NOT NULL,
    username VARCHAR(64),
    password_hash TEXT NOT NULL,
    full_name VARCHAR(200) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT ck_users_email_nonempty CHECK (length(btrim(email)) > 0),
    CONSTRAINT ck_users_full_name_nonempty CHECK (length(btrim(full_name)) > 0)
);

CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    token_hash TEXT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT uq_refresh_tokens_token_hash UNIQUE (token_hash),
    CONSTRAINT ck_refresh_tokens_hash_nonempty CHECK (length(btrim(token_hash)) > 0),
    CONSTRAINT ck_refresh_tokens_expiry CHECK (expires_at > created_at)
);

CREATE TABLE tasks (
    id UUID PRIMARY KEY,
    owner_id UUID NOT NULL,
    title VARCHAR(300) NOT NULL,
    description TEXT,
    deadline TIMESTAMPTZ,
    status VARCHAR(20) NOT NULL,
    priority VARCHAR(20) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_tasks_owner
        FOREIGN KEY (owner_id) REFERENCES users(id),
    CONSTRAINT ck_tasks_title_nonempty CHECK (length(btrim(title)) > 0),
    CONSTRAINT ck_tasks_status CHECK (status IN ('Todo', 'InProgress', 'Done')),
    CONSTRAINT ck_tasks_priority CHECK (priority IN ('Low', 'Medium', 'High'))
);

CREATE TABLE tags (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    CONSTRAINT uq_tags_name UNIQUE (name),
    CONSTRAINT ck_tags_name_nonempty CHECK (length(btrim(name)) > 0)
);

CREATE TABLE task_tag_assignments (
    task_id UUID NOT NULL,
    tag_id UUID NOT NULL,
    CONSTRAINT pk_task_tag_assignments PRIMARY KEY (task_id, tag_id),
    CONSTRAINT fk_task_tag_assignments_task
        FOREIGN KEY (task_id) REFERENCES tasks(id) ON DELETE CASCADE,
    CONSTRAINT fk_task_tag_assignments_tag
        FOREIGN KEY (tag_id) REFERENCES tags(id) ON DELETE CASCADE
);

CREATE TABLE meetings (
    id UUID PRIMARY KEY,
    organizer_id UUID NOT NULL,
    title VARCHAR(300) NOT NULL,
    description TEXT,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    location TEXT,
    cancelled_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_meetings_organizer
        FOREIGN KEY (organizer_id) REFERENCES users(id),
    CONSTRAINT ck_meetings_title_nonempty CHECK (length(btrim(title)) > 0),
    CONSTRAINT ck_meetings_time_range CHECK (end_time >= start_time)
);

CREATE TABLE meeting_participants (
    meeting_id UUID NOT NULL,
    user_id UUID NOT NULL,
    participant_status VARCHAR(20),
    participant_location TEXT,
    added_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_meeting_participants PRIMARY KEY (meeting_id, user_id),
    CONSTRAINT fk_meeting_participants_meeting
        FOREIGN KEY (meeting_id) REFERENCES meetings(id) ON DELETE CASCADE,
    CONSTRAINT fk_meeting_participants_user
        FOREIGN KEY (user_id) REFERENCES users(id),
    CONSTRAINT ck_meeting_participant_status
        CHECK (participant_status IS NULL OR participant_status IN ('online', 'offline'))
);

CREATE TABLE meeting_tag_assignments (
    meeting_id UUID NOT NULL,
    tag_id UUID NOT NULL,
    CONSTRAINT pk_meeting_tag_assignments PRIMARY KEY (meeting_id, tag_id),
    CONSTRAINT fk_meeting_tag_assignments_meeting
        FOREIGN KEY (meeting_id) REFERENCES meetings(id) ON DELETE CASCADE,
    CONSTRAINT fk_meeting_tag_assignments_tag
        FOREIGN KEY (tag_id) REFERENCES tags(id) ON DELETE CASCADE
);

CREATE TABLE meeting_links (
    id UUID PRIMARY KEY,
    meeting_id UUID NOT NULL,
    url TEXT NOT NULL,
    position INTEGER NOT NULL,
    CONSTRAINT fk_meeting_links_meeting
        FOREIGN KEY (meeting_id) REFERENCES meetings(id) ON DELETE CASCADE,
    CONSTRAINT ck_meeting_links_url_nonempty CHECK (length(btrim(url)) > 0),
    CONSTRAINT ck_meeting_links_position CHECK (position >= 0),
    CONSTRAINT uq_meeting_links_position UNIQUE (meeting_id, position)
);

-- Индексы для основных фильтров и соединений.
CREATE INDEX ix_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX ix_refresh_tokens_active_expiry ON refresh_tokens(user_id, expires_at)
    WHERE revoked_at IS NULL;

CREATE INDEX ix_tasks_owner_created ON tasks(owner_id, created_at DESC, id);
CREATE INDEX ix_tasks_owner_status ON tasks(owner_id, status);
CREATE INDEX ix_tasks_owner_priority ON tasks(owner_id, priority);
CREATE INDEX ix_tasks_owner_deadline ON tasks(owner_id, deadline);
CREATE INDEX ix_tasks_deadline ON tasks(deadline) WHERE deadline IS NOT NULL;

CREATE INDEX ix_task_tag_assignments_tag_id ON task_tag_assignments(tag_id);

CREATE INDEX ix_meetings_organizer_start ON meetings(organizer_id, start_time);
CREATE INDEX ix_meetings_start_end ON meetings(start_time, end_time);
CREATE INDEX ix_meetings_active_start ON meetings(start_time) WHERE cancelled_at IS NULL;

CREATE INDEX ix_meeting_participants_user_id ON meeting_participants(user_id);
CREATE INDEX ix_meeting_tag_assignments_tag_id ON meeting_tag_assignments(tag_id);

COMMIT;

-- Примечания:
-- 1. UUID генерируется приложением либо функцией PostgreSQL, подключённой отдельно.
--    Здесь намеренно нет зависимости от расширения pgcrypto/uuid-ossp.
-- 2. DEFAULT CURRENT_TIMESTAMP не обновляет updated_at при UPDATE автоматически.
--    Приложение должно обновлять его либо следует добавить триггер.
-- 3. Индексирование массива тегов реализовано через таблицы-связки.
-- 4. Проверка прав доступа к записям не заменяется внешними ключами.
