# Redis Sink Connector для Change Data Capture (CDC)

## Обзор

Этот проект демонстрирует использование Change Data Capture (CDC) с PostgreSQL, Kafka и Redis. Система отслеживает изменения данных в PostgreSQL и автоматически синхронизирует их с Redis через Kafka.

## Компоненты

- **PostgreSQL**: источник данных
- **Debezium**: коннектор для отслеживания изменений в PostgreSQL
- **Kafka**: система обмена сообщениями для передачи событий CDC
- **Zookeeper**: координационный сервис для Kafka
- **Schema Registry**: хранилище схем для сериализации/десериализации сообщений Kafka
- **Kafka Connect**: фреймворк для интеграции Kafka с внешними системами
- **Redis**: хранилище ключ-значение для кэширования данных

## Автоматическая настройка коннекторов

Система использует специальный сервис, определенный в `docker-compose.yml`, который автоматически настраивает необходимые коннекторы при запуске. Этот сервис:

1. Ожидает доступности Kafka Connect
2. Проверяет наличие существующих коннекторов
3. Регистрирует новые коннекторы при необходимости

### Конфигурация Redis

Redis настроен для работы в режиме Append-Only File (AOF), что обеспечивает долговечность данных. Конфигурация Redis определена в файле `redis.conf`.

### Конфигурация Redis Sink Connector

Коннектор Redis настроен для получения данных из топиков Kafka, соответствующих таблицам PostgreSQL:

```json
{
  "name": "redis-sink",
  "config": {
    "connector.class": "io.confluent.connect.redis.RedisSinkConnector",
    "tasks.max": "1",
    "topics": "postgres.public.students",
    "redis.uri": "redis://redis:6379",
    "redis.client.mode": "standalone",
    "key.converter": "org.apache.kafka.connect.storage.StringConverter",
    "value.converter": "io.confluent.connect.avro.AvroConverter",
    "value.converter.schema.registry.url": "http://schema-registry:8081",
    "insert.mode": "upsert",
    "delete.enabled": "true",
    "redis.kcql": "INSERT INTO student_ SELECT * FROM postgres.public.students PK id"
  }
}
```

Обратите внимание на следующие важные параметры:
- `redis.kcql`: определяет, как данные из Kafka будут сохраняться в Redis
- `delete.enabled`: позволяет обрабатывать события удаления
- `insert.mode`: режим вставки данных (upsert объединяет операции вставки и обновления)

## Запуск системы

1. Запустите все сервисы с помощью Docker Compose:

```bash
docker-compose up -d
```

2. Проверьте статус коннекторов:

```bash
curl -X GET http://localhost:8083/connectors/redis-sink/status
```

Или в PowerShell:

```powershell
Invoke-RestMethod -Uri "http://localhost:8083/connectors/redis-sink/status" -Method Get
```

3. Проверьте данные в Redis:

```bash
docker exec -it redis redis-cli keys "*"
```

## Операции CDC

Система поддерживает следующие операции CDC:

### 1. Create (Создание)

При добавлении новой записи в PostgreSQL, соответствующая запись создается в Redis:

```bash
docker exec -it postgres psql -U admin -d mydb -c "INSERT INTO students (id, fullname, group_id) VALUES (15, 'New Student', 4);"
```

Проверка в Redis (через 5 секунд):

```bash
docker exec -it redis redis-cli get student_15
```

### 2. Update (Обновление)

При обновлении записи в PostgreSQL, соответствующая запись обновляется в Redis:

```bash
docker exec -it postgres psql -U admin -d mydb -c "UPDATE students SET fullname = 'Updated Student' WHERE id = 15;"
```

Проверка в Redis (через 5 секунд):

```bash
docker exec -it redis redis-cli get student_15
```

В обновленной записи поле `__op` будет иметь значение `"u"` (update).

### 3. Delete (Удаление)

При удалении записи в PostgreSQL, соответствующая запись помечается как удаленная в Redis:

```bash
docker exec -it postgres psql -U admin -d mydb -c "DELETE FROM students WHERE id = 15;"
```

Проверка в Redis (через 5 секунд):

```bash
docker exec -it redis redis-cli get student_15
```

В удаленной записи поле `__deleted` будет иметь значение `"true"`, а поле `__op` будет иметь значение `"d"` (delete).

## Формат данных в Redis

Данные в Redis хранятся в формате JSON и включают следующие поля:
- Поля из исходной таблицы PostgreSQL (например, `id`, `fullname`, `group_id`)
- Метаданные CDC:
  - `__source_ts_ms`: временная метка события
  - `__deleted`: флаг удаления (true/false)
  - `__lsn`: Log Sequence Number
  - `__op`: тип операции (c - create, u - update, d - delete)
  - `__table`: имя исходной таблицы

Пример записи:
```json
{
  "group_id": 4,
  "id": 14,
  "fullname": "Updated Test Student",
  "__source_ts_ms": 1750341102441,
  "__deleted": "false",
  "__lsn": 29959272,
  "__op": "u",
  "date_of_recipient": null,
  "__table": "students"
}
```

## Устранение неполадок

### Проверка статуса коннектора

```bash
curl -X GET http://localhost:8083/connectors/redis-sink/status
```

В PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:8083/connectors/redis-sink/status" -Method Get
```

### Просмотр логов

```bash
docker logs kafka-connect
```

### Перезапуск коннектора

```bash
curl -X POST http://localhost:8083/connectors/redis-sink/restart
```

В PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:8083/connectors/redis-sink/restart" -Method Post
```

### Проверка данных в Redis

Список всех ключей:
```bash
docker exec -it redis redis-cli keys "*"
```

Получение значения по ключу:
```bash
docker exec -it redis redis-cli get student_14
``` 