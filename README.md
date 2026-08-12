# TimescaleResults

ASP.NET Core Web API для загрузки CSV-файлов с измерениями, расчёта статистики и сохранения результатов в PostgreSQL.

## Технологии

* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL 16
* CsvHelper
* Swagger / OpenAPI
* xUnit
* Docker Compose

## Требования

Для локального запуска необходимы:

* .NET 10 SDK
* Docker Desktop
* `dotnet-ef` версии 10.0.10

Если `dotnet-ef` ещё не установлен:

```bash
dotnet tool install --global dotnet-ef --version 10.0.10
```

Если установлен другой вариант версии:

```bash
dotnet tool update --global dotnet-ef --version 10.0.10
```

## Локальный запуск

Склонируйте репозиторий и откройте терминал в его корневой директории.

### 1. Восстановление зависимостей

```bash
dotnet restore TimescaleResults.slnx
```

### 2. Запуск PostgreSQL

```bash
docker compose up -d
```

Проверить состояние контейнера:

```bash
docker compose ps
```

Параметры локального PostgreSQL заданы в `compose.yaml`.

Development connection string находится в:

```text
src/TimescaleResults.Api/appsettings.Development.json
```

и соответствует настройкам из `compose.yaml`.

### 3. Применение миграций

```bash
dotnet ef database update --project src/TimescaleResults.Api
```

### 4. Запуск API

```bash
dotnet run --project src/TimescaleResults.Api --launch-profile http
```

По умолчанию API доступен по адресу:

```text
http://localhost:5031
```

Swagger UI:

```text
http://localhost:5031/swagger
```

OpenAPI-документ:

```text
http://localhost:5031/openapi/v1.json
```

## Формат CSV

CSV-файлы используют `;` в качестве разделителя.

Обязательный заголовок:

```text
Date;ExecutionTime;Value
```

Пример файла:

```csv
Date;ExecutionTime;Value
2026-08-10T12:00:00Z;1.5;10
2026-08-10T12:00:05Z;2;20
2026-08-10T12:00:10Z;3.5;30
```

Где:

* `Date` — время начала операции;
* `ExecutionTime` — время выполнения;
* `Value` — числовое значение.

## Правила валидации CSV

При загрузке проверяются следующие условия:

* заголовок должен быть `Date;ExecutionTime;Value`;
* файл должен содержать от 1 до 10 000 строк данных включительно;
* каждое поле обязательно;
* `Date` не может быть раньше `2000-01-01T00:00:00Z`;
* `Date` не может быть позже текущего времени;
* даты нормализуются в UTC;
* `ExecutionTime` не может быть отрицательным;
* `Value` не может быть отрицательным;
* числовые значения используют invariant culture и `.` в качестве десятичного разделителя;
* повреждённые строки отклоняются;
* строки с лишними столбцами отклоняются.

Если хотя бы одна строка невалидна, весь файл считается невалидным и изменения в базе данных не выполняются.

## API

### Загрузка CSV-файла

```http
POST /api/files
Content-Type: multipart/form-data
```

Имя multipart-поля:

```text
file
```

При успешной загрузке API возвращает:

```text
204 No Content
```

При ошибке валидации CSV:

```text
400 Bad Request
```

Перед сохранением API полностью:

1. разбирает CSV;
2. выполняет валидацию;
3. рассчитывает статистику;
4. сохраняет исходные значения и рассчитанный результат.

Если файл с таким же именем уже существует, предыдущая версия полностью заменяется новой.

Замена выполняется внутри транзакции PostgreSQL, поэтому частичное обновление данных невозможно.

Одновременные загрузки файлов с одинаковым именем сериализуются с помощью PostgreSQL transaction-scoped advisory lock.

### Получение результатов

```http
GET /api/results
```

Все query-параметры являются необязательными.

| Параметр                   | Описание                              |
| -------------------------- | ------------------------------------- |
| `fileName`                 | Точное имя файла                      |
| `minDateFrom`              | Минимальная дата первой операции      |
| `minDateTo`                | Максимальная дата первой операции     |
| `averageValueFrom`         | Минимальное среднее значение          |
| `averageValueTo`           | Максимальное среднее значение         |
| `averageExecutionTimeFrom` | Минимальное среднее время выполнения  |
| `averageExecutionTimeTo`   | Максимальное среднее время выполнения |

Фильтры можно использовать независимо друг от друга или комбинировать.

Пример:

```http
GET /api/results?averageValueFrom=100&averageValueTo=500&averageExecutionTimeTo=20
```

Если нижняя граница диапазона больше верхней, API возвращает:

```text
400 Bad Request
```

### Получение последних значений файла

```http
GET /api/files/{fileName}/values
```

Endpoint возвращает до 10 последних значений указанного файла.

Записи сортируются от более новых к более старым по полю `Date`.

Если несколько строк имеют одинаковое значение `Date`, для детерминированной сортировки используется `Id`.

Пример:

```http
GET /api/files/measurements.csv/values
```

Если файл с указанным именем не найден, возвращается пустой массив:

```json
[]
```

## Рассчитываемая статистика

Для каждого загруженного файла рассчитываются и сохраняются:

* диапазон времени между самой ранней и самой поздней операцией в секундах;
* дата самой ранней операции;
* среднее время выполнения;
* среднее значение;
* медиана;
* максимальное значение;
* минимальное значение.

## База данных

В приложении используются две основные сущности:

* `Results` — рассчитанный результат для загруженного файла;
* `Values` — исходные строки CSV, связанные с результатом.

Поле `Results.FileName` является уникальным.

При удалении `Result` связанные с ним `Values` удаляются каскадно.

Повторная загрузка файла выполняется внутри транзакции.

Если сохранение новой версии завершается ошибкой, предыдущая версия остаётся в базе данных без изменений.

Для защиты от одновременной замены одного и того же файла используется PostgreSQL advisory lock, привязанный к имени файла.

## Тесты и проверки

Запустить тесты:

```bash
dotnet test TimescaleResults.slnx
```

Собрать solution:

```bash
dotnet build TimescaleResults.slnx
```

Проверить форматирование:

```bash
dotnet format TimescaleResults.slnx --verify-no-changes
```

Проверить зависимости на известные уязвимости:

```bash
dotnet package list TimescaleResults.slnx --include-transitive --vulnerable
```

## Остановка PostgreSQL

Остановить контейнеры с сохранением данных:

```bash
docker compose down
```

Остановить контейнеры и удалить локальный volume базы данных:

```bash
docker compose down -v
```
