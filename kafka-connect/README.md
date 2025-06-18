# Kafka Connect с Debezium для CDC

Этот проект настраивает Kafka Connect с Debezium для реализации Change Data Capture (CDC) из PostgreSQL в Kafka, а затем в Elasticsearch и Redis.

## Компоненты

- **Zookeeper**: Координация Kafka кластера
- **Kafka Broker**: Брокер сообщений
- **Schema Registry**: Хранение и управление схемами
- **Kafka Connect**: Фреймворк для потоковой передачи данных
- **Control Center**: Веб-интерфейс для управления Kafka экосистемой

## Коннекторы

1. **Debezium PostgreSQL Source**: Захватывает изменения из PostgreSQL и отправляет их в Kafka
2. **Elasticsearch Sink**: Передает данные из Kafka в Elasticsearch
3. **Redis Sink**: Передает данные из Kafka в Redis

## Запуск

1. Запустите инфраструктуру:
   ```
   docker-compose up -d
   ```

2. Дождитесь запуска всех сервисов (1-2 минуты)

3. Запустите коннекторы:
   ```
   chmod +x start-connectors.sh
   ./start-connectors.sh
   ```

## Проверка работы

1. **Control Center**: http://localhost:9021
   - Проверьте топики и коннекторы

2. **Kafka Connect API**:
   ```
   curl -X GET http://localhost:8083/connectors
   ```

3. **Проверка CDC**:
   - Добавьте/измените данные в PostgreSQL
   - Проверьте появление данных в Kafka топиках
   - Проверьте синхронизацию с Elasticsearch и Redis

## Конфигурация коннекторов

Конфигурации коннекторов находятся в директории `config/kafka-connect/`:

- `postgres-source.json`: Debezium PostgreSQL Source
- `elasticsearch-sink.json`: Elasticsearch Sink
- `redis-sink.json`: Redis Sink 