-- Настройка параметров для CDC
ALTER SYSTEM SET wal_level = 'logical';
ALTER SYSTEM SET max_wal_senders = '10';
ALTER SYSTEM SET max_replication_slots = '10';
ALTER SYSTEM SET wal_keep_size = '512MB';

-- Создание публикации для CDC
CREATE PUBLICATION dbz_publication FOR ALL TABLES;

-- Создание пользователя для репликации (опционально)
-- CREATE ROLE debezium REPLICATION LOGIN PASSWORD 'debezium';
-- GRANT SELECT ON ALL TABLES IN SCHEMA public TO debezium;
-- GRANT USAGE ON SCHEMA public TO debezium;
-- ALTER PUBLICATION dbz_publication OWNER TO debezium;