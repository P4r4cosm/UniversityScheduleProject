FROM confluentinc/cp-kafka-connect-base:7.6.1

# Установка плагина Debezium для PostgreSQL
#RUN mkdir -p /tmp/debezium-connector && \
#    cd /tmp/debezium-connector && \
#    curl -LO https://repo1.maven.org/maven2/io/debezium/debezium-connector-postgres/2.5.0.Final/debezium-connector-postgres-2.5.0.Final-plugin.tar.gz && \
#    tar -xvf debezium-connector-postgres-2.5.0.Final-plugin.tar.gz -C /usr/share/java/ && \
#    rm -rf /tmp/debezium-connector

# Установка плагина Debezium для PostgreSQL со всеми зависимостями
RUN mkdir -p /usr/share/java/debezium-connector-postgres && \
    cd /usr/share/java/debezium-connector-postgres && \
    curl -LO https://repo1.maven.org/maven2/io/debezium/debezium-connector-postgres/2.5.0.Final/debezium-connector-postgres-2.5.0.Final-plugin.tar.gz && \
    tar -xvf debezium-connector-postgres-2.5.0.Final-plugin.tar.gz && \
    rm debezium-connector-postgres-2.5.0.Final-plugin.tar.gz

# Установка плагина для Elasticsearch
RUN confluent-hub install --no-prompt confluentinc/kafka-connect-elasticsearch:14.0.12

# Установка плагина для Redis (Lenses.io stream-reactor)
RUN mkdir -p /tmp/kafka-connect-redis && \
    cd /tmp/kafka-connect-redis && \
    curl -LO https://github.com/lensesio/stream-reactor/releases/download/3.0.1/kafka-connect-redis-3.0.1-2.5.0-all.tar.gz && \
    tar -xvf kafka-connect-redis-3.0.1-2.5.0-all.tar.gz -C /usr/share/java/ && \
    rm -rf /tmp/kafka-connect-redis

# Установка плагина для Neo4j
RUN confluent-hub install --no-prompt neo4j/kafka-connect-neo4j:5.1.12

# Рабочая директория
WORKDIR /usr/share/java

# Установка прав для запуска
USER root
RUN chmod -R 777 /etc/kafka-connect/
USER appuser 