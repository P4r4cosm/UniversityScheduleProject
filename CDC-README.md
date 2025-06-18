# Настройка CDC (Change Data Capture) с Kafka и Debezium

Этот проект демонстрирует настройку Change Data Capture (CDC) с использованием Apache Kafka и Debezium для захвата изменений из PostgreSQL и их передачи в Elasticsearch и Redis.

## Архитектура

![CDC Architecture](Схемы/cdc-architecture.png)

Основные компоненты:
- **PostgreSQL** с включенным механизмом CDC
- **Apache Kafka** (кластер брокеров)
- **Kafka Connect** с плагинами:
  - Debezium PostgreSQL Connector
  - Elasticsearch Sink Connector
  - Redis Sink Connector
- **Elasticsearch** для хранения и индексации данных
- **Redis** для кэширования данных

## Настройка и запуск

### 1. Запуск инфраструктуры

```bash
docker-compose up -d
```

### 2. Проверка готовности PostgreSQL для CDC

```bash
# Подключение к PostgreSQL
docker exec -it postgres psql -U admin -d mydb

# Проверка настроек WAL
SHOW wal_level;  # Должно быть "logical"

# Проверка публикации
SELECT * FROM pg_publication;

# Выход
\q
```

### 3. Генерация тестовых данных

Используйте сервис University Schedule Generator для создания тестовых данных:

```bash
curl -X POST http://localhost:8100/generate -H "Content-Type: application/json" -d '{
  "specialtiesCount": 5,
  "universityCount": 2,
  "institutionCount": 3,
  "departmentCount": 5,
  "groupCount": 10,
  "studentCount": 50,
  "courseCount": 10
}'
```

### 4. Запуск коннекторов

```bash
chmod +x start-connectors.sh
./start-connectors.sh
```

### 5. Проверка работы

#### Проверка Kafka Connect

```bash
# Список коннекторов
curl -X GET http://localhost:8083/connectors

# Статус PostgreSQL коннектора
curl -X GET http://localhost:8083/connectors/postgres-source/status
```

#### Проверка Kafka топиков

Откройте Control Center по адресу http://localhost:9021 и проверьте созданные топики:
- postgres.public.students
- postgres.public.courses
- и другие

#### Проверка Elasticsearch

```bash
# Проверка индексов
curl -X GET http://localhost:9200/_cat/indices

# Проверка данных в индексе students
curl -X GET http://localhost:9200/students/_search?pretty
```

#### Проверка Redis

```bash
# Подключение к Redis
docker exec -it redis redis-cli

# Проверка ключей
KEYS *

# Проверка данных студента
HGETALL students-1
```

## Дополнительные настройки

### Добавление таблиц в публикацию

```sql
ALTER PUBLICATION dbz_publication ADD TABLE new_table;
```

### Изменение конфигурации коннекторов

Конфигурации находятся в директории `config/kafka-connect/`:
- `postgres-source.json`
- `elasticsearch-sink.json`
- `redis-sink.json`

После изменения конфигурации перезапустите коннектор:

```bash
curl -X DELETE http://localhost:8083/connectors/connector-name
curl -X POST -H "Content-Type: application/json" --data @config/kafka-connect/connector-config.json http://localhost:8083/connectors
``` 