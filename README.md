# UniversityScheduleProject
## Описание
Система состоит баз данных, в которых дублируются данные для более оптимального поиска, микросервисов для поиска выполнения конкретных запросов и gateway также, отвечающего за ауентификацию.
Также реализован генератор, заполняющий базы данных всем необходимым для проверки работы сервиса.
## Используемые технологии: 
*  ASP.NET Core
*  Docker
*  EntityFramework Core
*  PostgreSQL
*  Redis
*  Neo4j
*  MongoDb
*  ElasticSearch
*  YARP
* Swagger
  
## Архитектура
### C4 Context
![image](https://github.com/user-attachments/assets/82995a20-28db-4280-afed-e8c6ef4c1075)
### C4 Container
![image](https://github.com/user-attachments/assets/8b792b52-1417-4983-bebd-7ed9982284ae)
### C4 Component
![image](https://github.com/user-attachments/assets/4715eccf-f880-44fc-ba10-758838eac517)
### DFD
![image](https://github.com/user-attachments/assets/24d89f66-1950-4637-afb9-c238fb50f2bc)


## Логическая схема данных:
![image](https://github.com/user-attachments/assets/e0793619-1d0b-4b30-acdd-a4d37059efaf)

## Физические схемы:
### Postgres
![image](https://github.com/user-attachments/assets/af149eb2-4ff2-4122-9ac5-e56babf57402)
### Redis
![image](https://github.com/user-attachments/assets/ef2f896d-7129-4889-a7e8-996eb3ecbff5)
### MongoDb
![image](https://github.com/user-attachments/assets/82958844-4d75-46c9-9eb8-213b2d9b6561)
### Neo4j
![image](https://github.com/user-attachments/assets/edca66df-2bcb-45f1-9d83-b88ab15eda84)
### ElasticSearch
![image](https://github.com/user-attachments/assets/f0fb795c-cc20-4096-a508-50d5ad339a1b)


## Запуск
Для запуска проекта необходимо скачать или клонировать его и выполнить в папке с проектом:
 ```docker compose up -d --build```
 


