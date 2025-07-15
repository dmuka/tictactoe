# TicTacToe API

[English Version](README_EN.md)

## Оглавление
- [Архитектурные решения](#архитектурные-решения)
  - [Clean Architecture](#clean-architecture)
  - [CQRS](#cqrs-command-query-responsibility-segregation)
  - [Оптимистическая блокировка](#оптимистическая-блокировка)
- [API Endpoints](#api-endpoints)
  - [GET /api/games](#get-apigames)
  - [POST /api/games](#post-apigames)
  - [GET /api/games/{id}](#get-apigamesid)
  - [POST /api/games/{id}/moves](#post-apigamesidmoves)
- [Модель данных](#модель-данных)
  - [GameDto](#gamedto)
  - [Правила игры](#правила-игры)
- [Настройки игры](#настройки-игры)
- [Конфигурация приложения](#конфигурация-приложения)
  - [appsettings.json](#appsettingsjson)
  - [Переменные окружения](#переменные-окружения)
  - [Настройка через Docker](#настройка-через-docker)
- [Запуск проекта](#запуск-проекта)
  - [Требования](#требования)
  - [Локальный запуск](#локальный-запуск)
  - [Запуск с использованием профиля окружения](#запуск-с-использованием-профиля-окружения)
  - [Запуск с использованием Docker](#запуск-с-использованием-docker-1)
- [Тестирование API](#тестирование-api)
- [Запуск тестов](#запуск-тестов)
- [Генерация отчета о покрытии кода тестами](#генерация-отчета-о-покрытии-кода-тестами)
- [Устранение неполадок](#устранение-неполадок)

## Архитектурные решения
Проект разработан с использованием следующих принципов и подходов:

### Clean Architecture
Проект разделен на несколько слоев:
- **Domain**: Содержит основную бизнес-логику и модели предметной области
- **Application**: Содержит сценарии использования (use cases) и логику приложения
- **Infrastructure**: Содержит реализации интерфейсов для взаимодействия с внешними системами
- **API**: Предоставляет HTTP API для взаимодействия с приложением

### CQRS (Command Query Responsibility Segregation)
Проект использует паттерн CQRS с помощью библиотеки MediatR:
- **Commands**: Для изменения состояния (CreateGame, MakeMove)
- **Queries**: Для получения данных (GetGame, GetAllGames)

### Оптимистическая блокировка
Для обеспечения согласованности данных при параллельном доступе используется оптимистическая блокировка с помощью версионирования (ETag):
- Каждая игра имеет свою версию
- При выполнении хода клиент должен предоставить текущую версию игры
- Если версия не совпадает, возвращается ошибка конфликта (409 Conflict)

## API Endpoints
### GET /api/games
Получает список всех игр.
**Ответ:**
- `200 OK`: Массив объектов игры

### POST /api/games
Создает новую игру.
**Ответ:**
- `201 Created`: Объект созданной игры

### GET /api/games/{id}
Получает информацию о конкретной игре.
**Параметры:**
- `id`: GUID идентификатор игры

**Ответ:**
- `200 OK`: Объект игры
- `404 Not Found`: Игра не найдена

### POST /api/games/{id}/moves
Совершает ход в игре.
**Параметры:**
- `id`: GUID идентификатор игры
- Заголовок `If-Match`: Версия игры в формате `W/"version"`

**Тело запроса:**
```json
{
  "player": "string",  // Символ игрока (X или O)
  "row": 0,            // Строка на доске (начиная с 0)
  "col": 0             // Столбец на доске (начиная с 0)
}
```

**Ответ:**

- `200 OK`: Обновленный объект игры
- `400 Bad Request`: Недопустимый ход или неверный формат данных
- `404 Not Found`: Игра не найдена
- `409 Conflict`: Конфликт версий (игра была изменена с момента последнего запроса)

## Модель данных
**GameDto**
```json
{
"id": "guid",
"board": [["", "", ""], ["", "", ""], ["", "", ""]],
"currentPlayer": "X|O",
"status": "InProgress|Draw|XPlayerWon|OPlayerWon",
"version": 0
}
```
## Правила игры
- Стандартные правила крестиков-ноликов
- Размер доски и количество символов для победы настраиваемые
- Минимальный размер доски: 3x3
- Условие победы: от 3 до размера доски символов в ряд
- В 10% случаев каждые 3 хода может произойти "случайный ход"
  
## Настройки игры
Настройки игры конфигурируются через опции в классе ``GameSettings``:
- ``BoardSize``: Размер игрового поля
- ``WinCondition``: Количество символов в ряд для победы

## Конфигурация приложения
**appsettings.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GameSettings": {
    "BoardSize": 3,
    "WinCondition": 3
  }
}
```
**Переменные окружения**

| Переменная | Описание |   Пример   |
|---|:---:|:---:|
| ASPNETCORE_ENVIRONMENT | Окружение | Production |
| GameSettings__BoardSize |	Размер поля |     3      |
| GameSettings__WinCondition | Условие победы |     	3     |

**Настройка через Docker**
```sh
docker run -p 8080:80 \
-e GameSettings__BoardSize=5 \
-e GameSettings__WinCondition=4 \
tictactoe-api
```

## Запуск проекта
**Требования**
- .NET 9.0 SDK
- Docker (опционально)

**Локальный запуск**
```shell
git clone https://github.com/dmuka/TicTacToe.git
cd TicTacToe
dotnet restore
dotnet build
cd src/API
dotnet run
```

**Docker**
```shell
docker-compose up -d
```

## Тестирование API
Можно использовать любой HTTP клиент или файл ``API.http`` в корне проекта.

```shell
# Создание игры
curl -X POST http://localhost:8080/api/games -H "Content-Type: application/json"

# Ход
curl -X POST http://localhost:8080/api/games/{id}/moves \
  -H "Content-Type: application/json" \
  -H "If-Match: W/\"1\"" \
  -d '{"player": "X", "row": 0, "col": 0}'
```

## Тестирование
```shell
dotnet test
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```
Или используйте ``bash`` скрипт ``coverage.sh`` в корне солюшена.
В git bash:
```sh

./coverage.sh
```

## Устранение неполадок
- Проверьте порты и настройки брандмауэра
- Убедитесь, что ``BoardSize`` ≥ 3
- Для отладки установите ``LogLevel: Debug`` в ``appsettings.Development.json``