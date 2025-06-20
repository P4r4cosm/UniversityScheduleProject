### Тестирование интеграции с Neo4j напрямую (без Kafka Connect)

Это руководство объясняет, как наполнить базу данных Neo4j данными напрямую из сервиса-генератора, минуя систему CDC (Kafka Connect). Это полезно для проверки работоспособности самого сервиса Neo4j, Cypher-запросов в генераторе и общей конфигурации.

#### Шаг 1: Проверка конфигурации сервиса-генератора

Перед запуском убедитесь, что сервис `University Schedule Generator` настроен для сохранения данных в Neo4j.

1.  Откройте файл `University Schedule Generator/Program.cs`.
2.  Убедитесь, что следующие строки **не закомментированы**:
    *   `builder.Services.AddNeo4j();` (в секции регистрации сервисов баз данных).
    *   Что сервис `Neo4jDataSaver` регистрируется в `DataSaverService` или напрямую (например, через `builder.Services.AddScoped<IDataSaver, Neo4jDataSaver>();`).

#### Шаг 2: Запуск окружения

Убедитесь, что все сервисы запущены. Если они не запущены, выполните команду в корне проекта:
```bash
docker-compose up -d
```

#### Шаг 3: Запуск генерации данных

Выполните следующий PowerShell-скрипт, чтобы отправить запрос на генерацию данных к сервису `university-schedule-generator`. Этот запрос создаст небольшое количество сущностей.

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5001/generate -Body '{
    "universities": 1,
    "institutes": 2,
    "departments": 2,
    "specialities": 3,
    "courses": 3,
    "groups": 4,
    "students": 50,
    "lectures": 10,
    "schedules": 10,
    "visits": 100,
    "materials": 5
}' -ContentType "application/json"
```
*Если команда выполнилась без вывода ошибок, это означает, что запрос на генерацию был успешно отправлен.*

#### Шаг 4: Проверка данных в Neo4j Browser

1.  Откройте Neo4j Browser в вашем веб-браузере: [http://localhost:7474](http://localhost:7474).
2.  Для входа используйте следующие учетные данные:
    *   **Username:** `neo4j`
    *   **Password:** Пароль можно найти в файле `docker-compose.yml` в секции `services.neo4j.environment` (значение переменной `NEO4J_AUTH`).
3.  После входа в систему выполните следующий Cypher-запрос, чтобы увидеть сгенерированные данные:
    ```cypher
    MATCH (n) RETURN n LIMIT 100;
    ```
    Вы должны увидеть граф с узлами (студенты, группы, лекции и т.д.). Если узлы отображаются, значит, прямое сохранение данных в Neo4j работает корректно. 