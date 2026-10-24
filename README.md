# Web Fórmula 1

Aplicación en C# que muestra el calendario, los resultados, las clasificaciones y las estadísticas de la Fórmula 1, con todos los horarios en **hora de Colombia (UTC-5)**, y que avisa por **Telegram** cuando está por empezar cada sesión.

Los datos salen de la API pública [Jolpica F1](https://api.jolpi.ca/) (formato Ergast) y la historia de cada escudería, de Wikipedia.

## Qué hace

- **Calendario con horarios en Colombia:** Libres 1, 2 y 3, clasificación, sprint y carrera de cada Gran Premio.
- **Resultados por carrera:** posición, parrilla, tiempo o estado, puntos y vuelta rápida.
- **Clasificación:** tabla de pilotos y de constructores por temporada.
- **Estadísticas de pilotos:** carreras, victorias, podios, poles, vueltas rápidas, abandonos y mejor resultado.
- **Escuderías:** pilotos actuales e historia del equipo.
- **Notificaciones por Telegram:**
  - Un resumen con todos los horarios del fin de semana, 24 horas antes de la primera sesión.
  - Un recordatorio unos minutos antes de cada sesión (15 por defecto).
- **Sincronización automática** con Jolpica cada 3 horas.

## Arquitectura

```
src/
  WebFormula1.Core            Entidades, DTOs compartidos y conversión a hora de Colombia
  WebFormula1.Infrastructure  EF Core (SQL Server), clientes de Jolpica y Wikipedia,
                              sincronización, notificaciones y trabajos programados
  WebFormula1.Api             API REST (ASP.NET Core), Swagger y Hangfire
  WebFormula1.Blazor          Interfaz web (Blazor WebAssembly)
```

- **Base de datos:** SQL Server con Entity Framework Core. Las migraciones se aplican solas al arrancar la API.
- **Trabajos programados (Hangfire):**

  | Trabajo | Frecuencia | Qué hace |
  |---|---|---|
  | `sync-current-season` | cada 3 horas | Descarga calendario, pilotos, equipos, clasificaciones y resultados |
  | `schedule-notifications` | cada 30 min | Crea o actualiza los avisos según los horarios actuales |
  | `send-notifications` | cada minuto | Envía los avisos cuya hora ya llegó |

- Si la base está vacía, la API descarga sola la temporada actual al arrancar.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express o LocalDB)
- Certificado de desarrollo de HTTPS de .NET: `dotnet dev-certs https --trust`

## Configuración

### Base de datos

La cadena de conexión está en `src/WebFormula1.Api/appsettings.json` y apunta a `CRISTHIAN\SQLEXPRESS`, el equipo de desarrollo original. **Cámbiala por tu servidor**, por ejemplo:

```
Server=(localdb)\\mssqllocaldb;Database=Formula1DB;Trusted_Connection=true;TrustServerCertificate=true;
```

Para no modificar el archivo versionado, puedes guardarla en los secretos de usuario:

```bash
cd src/WebFormula1.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "TU_CADENA"
```

### Telegram

1. En Telegram, habla con **@BotFather**, envía `/newbot` y guarda el token que te entrega.
2. Escríbele cualquier mensaje a tu bot nuevo (por ejemplo `/start`).
3. Obtén tu `ChatId` abriendo `https://api.telegram.org/bot<TU_TOKEN>/getUpdates` y copiando el valor de `"chat":{"id":...}`.
4. Guarda ambos datos como secretos de usuario. **Nunca los escribas en `appsettings.json` ni los subas al repositorio**:

```bash
cd src/WebFormula1.Api
dotnet user-secrets set "Telegram:BotToken" "TU_TOKEN"
dotnet user-secrets set "Telegram:ChatId" "TU_CHAT_ID"
```

5. Con la API en marcha, envía un mensaje de prueba: `POST https://localhost:7001/api/notifications/test`.

Si Telegram no está configurado, los avisos solo aparecen en el log de la API y como banner en la página de inicio.

Para cambiar con cuántos minutos de anticipación se avisa de cada sesión, ajusta en `appsettings.json`:

```json
"Notifications": { "SessionLeadMinutes": 15 }
```

## Cómo ejecutarla

Se necesitan dos terminales. Arranca primero la API.

```bash
cd src/WebFormula1.Api
dotnet run
```

```bash
cd src/WebFormula1.Blazor
dotnet run
```

| Servicio | URL |
|---|---|
| Web | https://localhost:55744 |
| API | https://localhost:7001 |
| Swagger | https://localhost:7001/swagger |
| Hangfire | https://localhost:7001/hangfire |

La web llama a la API en `https://localhost:7001/` (definido en `src/WebFormula1.Blazor/Program.cs`).

**Los avisos solo se envían mientras la API esté en ejecución.**

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/races/seasons` | Temporadas disponibles |
| GET | `/api/races/next` | Próxima carrera con sus sesiones |
| GET | `/api/races/{year}` | Calendario de una temporada |
| GET | `/api/races/{year}/{round}` | Detalle y resultados de una carrera |
| GET | `/api/standings/drivers/{year}` | Clasificación de pilotos |
| GET | `/api/standings/constructors/{year}` | Clasificación de constructores |
| GET | `/api/drivers/{year}/stats` | Estadísticas de pilotos |
| GET | `/api/teams/{year}` | Escuderías con pilotos e historia |
| GET | `/api/notifications/active` | Avisos vigentes del fin de semana |
| POST | `/api/notifications/test` | Envía un mensaje de prueba |
| POST | `/api/sync/season/{year}` | Fuerza la sincronización de una temporada |
| GET | `/api/apistatus/health` | Comprueba la conexión con Jolpica |

## Limitaciones conocidas

- Los puntos de la tabla de estadísticas de pilotos solo suman carreras dominicales (sin sprint), así que pueden diferir de la clasificación oficial.
- «Poles» se calcula como salidas desde la primera posición de la parrilla.
- La historia de las escuderías viene de Wikipedia y está en inglés.
- Jolpica limita las peticiones; la sincronización hace pocas llamadas, pero no la fuerces en bucle.

## Créditos

Datos de [Jolpica F1](https://github.com/jolpica/jolpica-f1) y [Wikipedia](https://www.wikipedia.org/).
