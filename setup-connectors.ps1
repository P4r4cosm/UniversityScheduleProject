Write-Host "Waiting for Kafka Connect to start..." -ForegroundColor Cyan

# Ждем, пока Kafka Connect станет доступен
$attempts = 0
$max_attempts = 30

while ($attempts -lt $max_attempts) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8083" -Method Get -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "Kafka Connect is available!" -ForegroundColor Green
            break
        }
    } catch {
        $attempts++
        Write-Host "Attempt ${attempts}/${max_attempts}: Kafka Connect is not available yet. Waiting..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
    
    if ($attempts -eq $max_attempts) {
        Write-Host "ERROR: Kafka Connect is not available after ${max_attempts} attempts!" -ForegroundColor Red
        exit 1
    }
}

# Даем дополнительное время для полной инициализации Kafka Connect
Write-Host "Giving Kafka Connect additional time to fully initialize..." -ForegroundColor Cyan
Start-Sleep -Seconds 10

# Функция для регистрации коннектора
function Register-Connector {
    param (
        [string]$ConnectorName,
        [string]$ConfigFile
    )
    
    Write-Host "Starting ${ConnectorName}..." -ForegroundColor Cyan
    
    # Проверяем, существует ли уже коннектор
    try {
        $existing = Invoke-WebRequest -Uri "http://localhost:8083/connectors/${ConnectorName}" -Method Get -UseBasicParsing -ErrorAction SilentlyContinue
        if ($existing.StatusCode -eq 200) {
            Write-Host "Connector ${ConnectorName} already exists. Skipping." -ForegroundColor Yellow
            return
        }
    } catch {
        # Коннектор не существует, продолжаем
    }
    
    # Регистрируем коннектор
    try {
        $config = Get-Content -Path $ConfigFile -Raw
        $response = Invoke-WebRequest -Uri "http://localhost:8083/connectors" -Method Post -Body $config -ContentType "application/json" -UseBasicParsing
        if ($response.StatusCode -eq 201 -or $response.StatusCode -eq 200) {
            Write-Host "Connector ${ConnectorName} started successfully." -ForegroundColor Green
        } else {
            Write-Host "Failed to start connector ${ConnectorName}. Response code: $($response.StatusCode)" -ForegroundColor Red
        }
    } catch {
        $errorMessage = $_.Exception.Message
        Write-Host "Error starting connector ${ConnectorName}: ${errorMessage}" -ForegroundColor Red
    }
}

# Регистрируем коннекторы
Register-Connector -ConnectorName "postgres-source" -ConfigFile ".\config\kafka-connect\postgres-source.json"
Register-Connector -ConnectorName "elasticsearch-sink" -ConfigFile ".\config\kafka-connect\elasticsearch-sink.json"
Register-Connector -ConnectorName "redis-sink" -ConfigFile ".\config\kafka-connect\redis-sink.json"
Register-Connector -ConnectorName "neo4j-sink" -ConfigFile ".\config\kafka-connect\neo4j-sink.json"

# Получаем список активных коннекторов
Write-Host "Listing active connectors:" -ForegroundColor Cyan
$connectors = Invoke-RestMethod -Uri "http://localhost:8083/connectors" -Method Get
$connectors | ConvertTo-Json

Write-Host "Done! Check Control Center at http://localhost:9021 for monitoring connectors." -ForegroundColor Green 