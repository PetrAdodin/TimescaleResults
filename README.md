# TimescaleResults

Web API для загрузки timescale-данных из CSV, расчёта статистики и хранения результатов в PostgreSQL.

## Стек

* .NET 10 / ASP.NET Core
* Entity Framework Core
* PostgreSQL 16
* CsvHelper
* Swagger / OpenAPI
* xUnit
* Docker Compose

## Запуск

Требуются .NET 10 SDK, Docker и `dotnet-ef`.

```bash
dotnet restore TimescaleResults.slnx
docker compose up -d
dotnet ef database update --project src/TimescaleResults.Api
dotnet run --project src/TimescaleResults.Api --launch-profile http
```

Swagger:

```text
http://localhost:5031/swagger
```

## CSV

Разделитель — `;`.

```csv
Date;ExecutionTime;Value
2026-08-10T12:00:00Z;1.5;10
2026-08-10T12:00:05Z;2;20
2026-08-10T12:00:10Z;3.5;30
```

Основные ограничения:

* от 1 до 10 000 строк;
* все поля обязательны;
* `Date` — не раньше `01.01.2000` и не позже текущего времени;
* `ExecutionTime >= 0`;
* `Value >= 0`;
* десятичный разделитель — `.`;
* если хотя бы одна строка невалидна, файл целиком отклоняется.

## API

### Загрузка файла

```http
POST /api/files
Content-Type: multipart/form-data
```

Имя поля формы: `file`.

Повторная загрузка файла с тем же именем полностью заменяет предыдущие данные.

### Результаты

```http
GET /api/results
```

Доступные фильтры:

* `fileName`
* `minDateFrom`
* `minDateTo`
* `averageValueFrom`
* `averageValueTo`
* `averageExecutionTimeFrom`
* `averageExecutionTimeTo`

Фильтры можно комбинировать.

Пример:

```http
GET /api/results?averageValueFrom=100&averageValueTo=500
```

### Последние значения

```http
GET /api/files/{fileName}/values
```

Возвращает до 10 последних записей файла по `Date`.

## Проверка

```bash
dotnet build TimescaleResults.slnx
dotnet test TimescaleResults.slnx
dotnet format TimescaleResults.slnx --verify-no-changes
```

Остановить PostgreSQL:

```bash
docker compose down
```
