# Настройка Redis Sink Connector для CDC

## Обзор

Этот документ описывает настройку Redis Sink Connector для получения данных из Kafka, которые были захвачены из PostgreSQL с помощью Debezium CDC.

## Компоненты

1. **PostgreSQL** с включенным CDC (Change Data Capture)
2. **Kafka и Zookeeper** для обработки потока событий
3. **Kafka Connect** с установленными коннекторами:
   - Debezium PostgreSQL Source Connector
   - Redis Sink Connector от Lenses.io (Stream Reactor)
4. **Redis** для хранения кэшированных данных

## Автоматическая настройка коннекторов

Система настроена на полностью автоматическую конфигурацию коннекторов при запуске контейнеров. Это реализовано через специальный сервис `connector-setup` в docker-compose.yml:

```yaml
connector-setup:
  image: curlimages/curl:latest
  container_name: connector-setup
  depends_on:
    - kafka-connect
    - postgres
    - redis
    - elasticsearch
  volumes:
    - ./config/kafka-connect:/config
  restart: on-failure
  entrypoint: ["/bin/sh", "-c"]
  command: |
    "
    # Функции для проверки доступности и регистрации коннекторов
    wait_for_service() { ... }
    register_connector() { ... }
    
    # Ждем, пока Kafka Connect станет доступен
    wait_for_service
    
    # Регистрируем коннекторы
    register_connector 'postgres-source' '/config/postgres-source.json'
    register_connector 'elasticsearch-sink' '/config/elasticsearch-sink.json'
    register_connector 'redis-sink' '/config/redis-sink.json'
    "
```

Этот сервис выполняет следующие функции:

1. **Ожидание готовности Kafka Connect** - проверяет доступность сервиса перед настройкой коннекторов
2. **Проверка существующих коннекторов** - не пытается создать уже существующие коннекторы
3. **Регистрация коннекторов** - автоматически настраивает все необходимые коннекторы
4. **Обработка ошибок** - перезапускается при сбоях благодаря политике `restart: on-failure`

Преимущества этого подхода:
- Не требуются внешние скрипты
- Автоматический запуск при старте системы
- Идемпотентность (безопасный повторный запуск)
- Интеграция в инфраструктуру Docker Compose

## Настройка Redis

Redis настроен в docker-compose.yml с включенным режимом сохранения AOF (Append-Only File):

```yaml
redis:
  build: ./redis
  container_name: redis
  restart: unless-stopped
  ports:
    - "6379:6379"
  volumes:
    - ./data/redis:/data
    - ./logs/redis:/var/log/redis
  command: redis-server --appendonly yes
```

## Настройка Redis Sink Connector

Конфигурация Redis Sink Connector находится в файле `config/kafka-connect/redis-sink.json`:

```json
{
    "name": "redis-sink",
    "config": {
        "connector.class": "com.datamountaineer.streamreactor.connect.redis.sink.RedisSinkConnector",
        "tasks.max": "1",
        "topics": "postgres.public.students,postgres.public.courses",
        "connect.redis.host": "redis",
        "connect.redis.port": "6379",
        "connect.redis.password": "",
        "connect.redis.timeout": "5000",
        "connect.redis.db": "0",
        "connect.redis.kcql": "INSERT INTO student- SELECT * FROM postgres.public.students PK id;INSERT INTO course- SELECT * FROM postgres.public.courses PK id",
        "connect.redis.sink.kcql.prefix.keys": "false",
        "connect.redis.sink.expire.keys": "false",
        "connect.redis.sink.expire.keys.time.sec": "0",
        "key.converter": "org.apache.kafka.connect.json.JsonConverter",
        "value.converter": "org.apache.kafka.connect.json.JsonConverter",
        "key.converter.schemas.enable": "true",
        "value.converter.schemas.enable": "true",
        "behavior.on.error": "log",
        "transforms": "unwrap,extractKey",
        "transforms.unwrap.type": "io.debezium.transforms.ExtractNewRecordState",
        "transforms.unwrap.drop.tombstones": "false",
        "transforms.unwrap.delete.handling.mode": "rewrite",
        "transforms.extractKey.type": "org.apache.kafka.connect.transforms.ExtractField$Key",
        "transforms.extractKey.field": "id"
    }
}
```

### Объяснение конфигурации

- `connector.class`: Класс коннектора из Stream Reactor
- `topics`: Топики Kafka, из которых читаются данные (students и courses)
- `connect.redis.kcql`: Язык запросов KCQL для определения, как данные из топиков Kafka отображаются на структуры Redis
- `transforms`: Преобразования для обработки событий CDC:
  - `unwrap`: Извлекает актуальное состояние записи из события CDC
  - `extractKey`: Извлекает поле `id` для использования в качестве ключа Redis

## Запуск и проверка

### Запуск системы

Для запуска всей системы выполните:

```
docker-compose up -d
```

После запуска всех контейнеров необходимо настроить коннекторы с помощью одного из скриптов:

#### Windows:
```
# Вариант 1: Запуск через batch-файл (самый простой способ)
setup-connectors.bat

# Вариант 2: Запуск PowerShell скрипта
powershell -ExecutionPolicy Bypass -File setup-connectors.ps1

# Вариант 3: Запуск из PowerShell
.\setup-connectors.ps1
```

### Проверка статуса настройки

Чтобы убедиться, что коннекторы настроены правильно:

```
# Проверка списка активных коннекторов
curl -s http://localhost:8083/connectors | jq .
```

В Windows через PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:8083/connectors" | ConvertTo-Json
```

### Проверка работы Redis Sink

После запуска проверьте наличие данных в Redis:

```
# Проверка наличия данных в Redis
docker exec -it redis redis-cli

# Проверка студентов
KEYS student:*
HGETALL student:1

# Проверка курсов
KEYS course:*
HGETALL course:1
```

### Мониторинг в Control Center

- Откройте http://localhost:9021
- Перейдите в раздел Connect -> redis-sink -> Tasks

## Проверка операций CDC

### Интерактивный тестовый скрипт

Для удобного тестирования операций CDC (Create, Update, Delete) и проверки их отражения в Redis, используйте интерактивный скрипт:

#### Windows:
```
# Вариант 1: Запуск через batch-файл (самый простой способ)
test-redis-cdc.bat

# Вариант 2: Запуск PowerShell скрипта
powershell -ExecutionPolicy Bypass -File test-redis-cdc.ps1

# Вариант 3: Запуск из PowerShell
.\test-redis-cdc.ps1
```

#### Linux/Mac:
```bash
# Сделать скрипт исполняемым
chmod +x test-redis-cdc.sh
# Запустить скрипт
./test-redis-cdc.sh
```

Скрипт предоставляет интерактивное меню для:
- Просмотра существующих данных в Redis
- Создания новых записей в PostgreSQL (студенты и курсы)
- Обновления существующих записей
- Удаления записей
- Проверки отражения этих изменений в Redis

Это позволяет легко и наглядно убедиться в корректной работе CDC для Redis.

### Ручная проверка операций

Если вы предпочитаете выполнять операции вручную:

#### Create (Создание)
Вставьте новую запись в PostgreSQL:
```sql
INSERT INTO students (full_name, date_of_recipient, group_id) 
VALUES ('Новый Студент', '2023-09-01', 1);
```

#### Update (Обновление)
Обновите существующую запись:
```sql
UPDATE students SET full_name = 'Измененное Имя' WHERE id = 1;
```

#### Delete (Удаление)
Удалите запись:
```sql
DELETE FROM students WHERE id = 2;
```

После каждой операции проверяйте состояние в Redis:
```
docker exec -it redis redis-cli
HGETALL student:1
```

## Устранение неполадок

1. Проверка статуса коннектора:
   
   В Windows через командную строку:
   ```
   curl -X GET http://localhost:8083/connectors/redis-sink/status
   ```
   
   В Windows через PowerShell:
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:8083/connectors/redis-sink/status" -Method Get
   ```

2. Просмотр логов Kafka Connect:
   ```
   docker logs kafka-connect
   ```

3. Просмотр логов сервиса автоматической настройки:
   ```
   docker logs connector-setup
   ```

4. Ручной перезапуск сервиса настройки:
   ```
   docker-compose restart connector-setup
   ```

5. Перезапуск коннектора при необходимости:
   
   В Windows через командную строку:
   ```
   curl -X POST http://localhost:8083/connectors/redis-sink/restart
   ```
   
   В Windows через PowerShell:
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:8083/connectors/redis-sink/restart" -Method Post
   ```

## CDC с использованием Redis

Этот проект демонстрирует использование Change Data Capture (CDC) с помощью Debezium и Redis.

### Предварительные требования

- Docker и Docker Compose
- PowerShell (для Windows) или Bash (для Linux/Mac)

### Запуск системы

1. Запустите Docker Compose:

```bash
docker-compose up -d
```

2. После запуска всех контейнеров, необходимо настроить коннекторы. Для этого можно использовать один из следующих способов:

#### Для Windows:
- Запустить batch-файл:
```
setup-connectors.bat
```

- Или выполнить PowerShell скрипт:
```powershell
powershell -ExecutionPolicy Bypass -File setup-connectors.ps1
```

- Или выполнить команды напрямую:
```powershell
# Проверить, что Kafka Connect доступен
curl -s http://localhost:8083

# Создать коннектор для PostgreSQL
curl -s -X POST -H "Content-Type: application/json" --data "@config/kafka-connect/postgres-source.json" http://localhost:8083/connectors

# Создать коннектор для Elasticsearch
curl -s -X POST -H "Content-Type: application/json" --data "@config/kafka-connect/elasticsearch-sink.json" http://localhost:8083/connectors

# Создать коннектор для Redis
curl -s -X POST -H "Content-Type: application/json" --data "@config/kafka-connect/redis-sink.json" http://localhost:8083/connectors
```

### Важные замечания по конфигурации коннекторов

При настройке коннекторов обратите внимание на следующие моменты:

1. В конфигурации PostgreSQL-коннектора (postgres-source.json) должно быть указано поле `topic.prefix`:
```json
"topic.prefix": "postgres"
```

2. В конфигурации PostgreSQL-коннектора для параметра `publication.autocreate.mode` следует использовать значение `all_tables` вместо `filtered`:
```json
"publication.autocreate.mode": "all_tables"
```

3. В конфигурации Redis-коннектора (redis-sink.json) имена полей должны начинаться с префикса `connect.redis.`:
```json
"connect.redis.host": "redis",
"connect.redis.port": "6379",
"connect.redis.kcql": "INSERT INTO student- SELECT * FROM postgres.public.students PK id;INSERT INTO course- SELECT * FROM postgres.public.courses PK id"
```

4. В KCQL-запросах для Redis обратите внимание на правильный синтаксис:
   - Для хранения данных в виде простых ключ-значение используйте формат: `INSERT INTO prefix- SELECT * FROM topic PK id`
   - Дефис после имени таблицы (`student-`) указывает, что это будет префикс для ключа
   - Не используйте двоеточие после имени таблицы
   - Если Redis требует аутентификацию, добавьте параметр `connect.redis.password`

### Проверка статуса настройки

Чтобы проверить, что коннекторы настроены правильно, выполните:

```bash
curl -s http://localhost:8083/connectors
```

Для Windows через PowerShell:
```powershell
Invoke-RestMethod -Uri "http://localhost:8083/connectors" -Method Get
```

Вы должны увидеть список настроенных коннекторов:
```
["postgres-source", "elasticsearch-sink", "redis-sink"]
```

### Доступ к данным

- **Kafka Control Center**: http://localhost:9021
- **Kibana**: http://localhost:5601
- **Neo4j Browser**: http://localhost:7474
- **Redis Commander**: http://localhost:8081

### Тестирование

1. Подключитесь к PostgreSQL и вставьте данные:

```bash
docker exec -it postgres psql -U admin -d mydb
```

```sql
INSERT INTO students (name, age, email) VALUES ('John Doe', 20, 'john@example.com');
```

2. Проверьте, что данные появились в Redis:

```bash
docker exec -it redis redis-cli
```

```
KEYS *
GET student-1
```

3. Проверьте, что данные появились в Elasticsearch:

```bash
curl -X GET "http://localhost:9200/students/_search?pretty"
```

### Остановка системы

```bash
docker-compose down
``` 