@echo off
echo Waiting for Kafka Connect to start...

REM Ждем, пока Kafka Connect станет доступен
set max_attempts=30
set attempts=0

:wait_loop
if %attempts% geq %max_attempts% (
    echo ERROR: Kafka Connect is not available after %max_attempts% attempts!
    exit /b 1
)

curl -s http://localhost:8083 >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo Kafka Connect is available!
) else (
    set /a attempts+=1
    echo Attempt %attempts%/%max_attempts%: Kafka Connect is not available yet. Waiting...
    timeout /t 5 /nobreak >nul
    goto wait_loop
)

REM Даем дополнительное время для полной инициализации Kafka Connect
echo Giving Kafka Connect additional time to fully initialize...
timeout /t 10 /nobreak >nul

REM Создаем коннекторы
echo Starting postgres-source connector...
curl -s -X POST -H "Content-Type: application/json" --data "@config/kafka-connect/postgres-source.json" http://localhost:8083/connectors

echo.
echo Starting elasticsearch-sink connector...
curl -s -X POST -H "Content-Type: application/json" --data "@config/kafka-connect/elasticsearch-sink.json" http://localhost:8083/connectors

echo.
echo Starting redis-sink connector...
curl -s -X POST -H "Content-Type: application/json" --data "@config/kafka-connect/redis-sink.json" http://localhost:8083/connectors

echo.
echo Listing active connectors:
curl -s http://localhost:8083/connectors

echo.
echo Done! Check Control Center at http://localhost:9021 for monitoring connectors. 