# 🏗️ Arquitectura - Web Fórmula 1

## Descripción General

La aplicación Web Fórmula 1 sigue una arquitectura de **capas limpias** con separación clara de responsabilidades. Está diseñada para ser escalable, mantenible y testeable.

## Diagrama de Capas

```
┌─────────────────────────────────────────┐
│         Interfaz de Usuario              │
│         (Blazor WebAssembly)             │
└────────────────┬──────────────────────────┘
                 │
┌────────────────┴──────────────────────────┐
│         API REST (ASP.NET Core)           │
│  Controllers, DTOs, Middleware            │
└────────────────┬──────────────────────────┘
                 │
┌────────────────┴──────────────────────────┐
│      Capa de Aplicación/Casos de Uso      │
│  Services, Sync, Notifications           │
└────────────────┬──────────────────────────┘
                 │
┌────────────────┴──────────────────────────┐
│         Capa de Dominio                   │
│  Modelos, Interfaces, Especificaciones   │
└────────────────┬──────────────────────────┘
                 │
┌────────────────┴──────────────────────────┐
│      Capa de Infraestructura              │
│  EF Core, API Client, Repositorios       │
└─────────────────────────────────────────┘
```

## Proyectos y Responsabilidades

### 1. WebFormula1.Core
**Ubicación**: `src/WebFormula1.Core`

**Responsabilidades**:
- Definir modelos de dominio
- Interfaces de servicios
- Especificaciones de negocio

**Contenido**:
- `/Models`: Entidades de dominio
  - `Driver.cs` - Piloto de F1
  - `Team.cs` - Escudería/Constructor
  - `Race.cs` - Carrera
  - `DriverResult.cs` - Resultado de piloto en carrera
  - `DriverStanding.cs` - Clasificación de piloto
  - `TeamStanding.cs` - Clasificación de constructor
  - `RaceNotification.cs` - Notificación de carrera

**Dependencias**:
- MediatR (opcional para CQRS)
- FluentValidation

### 2. WebFormula1.Infrastructure
**Ubicación**: `src/WebFormula1.Infrastructure`

**Responsabilidades**:
- Implementación de persistencia (EF Core)
- Integración con APIs externas
- Servicios de dominio

**Contenido**:

#### `/Data`
- `Formula1DbContext.cs` - Contexto de Entity Framework Core
- Configuración de modelos

#### `/Api`
- `IJolpicaF1ApiClient.cs` - Cliente Refit tipado para API Jolpica F1
- DTOs de respuesta de la API

#### `/Services`
- `Formula1SyncService.cs` - Sincronización de datos desde Jolpica F1
- `NotificationService.cs` - Gestión de notificaciones
- `INotificationProvider.cs` - Interfaz para proveedores de notificaciones

**Dependencias**:
- Entity Framework Core 8.0
- SQL Server
- Refit (HTTP Client)
- Polly (Resilencia)
- Hangfire (Background Jobs)

### 3. WebFormula1.Api
**Ubicación**: `src/WebFormula1.Api`

**Responsabilidades**:
- Exponer endpoints REST
- Validación de entrada
- Manejo de errores
- Logging

**Contenido**:

#### `/Controllers`
- `RacesController.cs` - Endpoints de carreras
- `DriversController.cs` - Endpoints de pilotos
- `TeamsController.cs` - Endpoints de escuderías
- (Próximamente: `StandingsController.cs`, `StatisticsController.cs`)

#### `/Middleware`
- `GlobalExceptionHandler.cs` - Manejo global de excepciones

#### `/Extensions`
- `ServiceCollectionExtensions.cs` - Extensiones para DI

#### Archivos de configuración
- `appsettings.json` - Configuración principal
- `appsettings.Development.json` - Configuración de desarrollo
- `Properties/launchSettings.json` - Configuración de ejecución

**Dependencias**:
- ASP.NET Core Web API
- Serilog
- Hangfire

### 4. WebFormula1.Blazor
**Ubicación**: `src/WebFormula1.Blazor`

**Responsabilidades**:
- Interfaz de usuario interactiva
- Llamadas a API
- Presentación de datos

**Contenido** (A implementar):

#### `/Pages`
- `Index.razor` - Página principal
- `Races.razor` - Listado de carreras
- `RaceDetails.razor` - Detalles de carrera
- `Drivers.razor` - Listado de pilotos
- `DriverDetails.razor` - Detalles de piloto
- `Teams.razor` - Listado de escuderías
- `TeamDetails.razor` - Detalles de escudería
- `Standings.razor` - Clasificaciones

#### `/Components`
- Componentes reutilizables (Card, Table, Modal, etc.)

#### `/Services`
- `ApiService.cs` - Cliente HTTP tipado para llamadas a API

**Dependencias**:
- Blazor WebAssembly
- Refit

## Flujo de Datos

### Flujo de Sincronización

```
Jolpica F1 API
      │
      ├─ GET /api/seasons/{year}
      ├─ GET /api/seasons/{year}/races
      ├─ GET /api/seasons/{year}/drivers
      ├─ GET /api/seasons/{year}/constructors
      └─ GET /api/seasons/{year}/standings
      │
      ▼
IJolpicaF1ApiClient (Refit)
      │
      ▼
Formula1SyncService
      │
      ├─ SyncRacesAsync()
      ├─ SyncDriversAsync()
      ├─ SyncTeamsAsync()
      └─ SyncStandingsAsync()
      │
      ▼
Formula1DbContext (EF Core)
      │
      ▼
SQL Server Database
```

### Flujo de Notificaciones

```
Formula1SyncService
      │
      ├─ Nueva carrera sincronizada
      │
      ▼
NotificationService
      │
      ├─ ScheduleRaceNotificationsAsync()
      │  └─ Crear RaceNotification para FP1, FP2, FP3, Q, Race
      │
      ▼
Hangfire (Trabajos programados)
      │
      ├─ SendPendingNotificationsAsync() (cada hora)
      │
      ▼
INotificationProvider
      │
      ├─ ConsoleNotificationProvider (actual)
      ├─ EmailNotificationProvider (próximamente)
      ├─ SmsNotificationProvider (próximamente)
      └─ WebPushNotificationProvider (próximamente)
```

### Flujo de API REST

```
Cliente Blazor
      │
      └─ GET /api/races/2026
      │
      ▼
RacesController.GetSeasonRaces()
      │
      ├─ Query en Formula1DbContext
      │
      ▼
SQL Server Database
      │
      ▼
JSON Response
      │
      ▼
Cliente Blazor (parsea y visualiza)
```

## Patrón de Inyección de Dependencias

La aplicación utiliza el contenedor nativo de .NET Core:

```csharp
// En Program.cs
builder.Services.AddInfrastructureServices(configuration);
builder.Services.AddApiServices();

// En appsettings.json
{
  "Logging": { ... },
  "ConnectionStrings": { ... },
  "JolpicaF1": { ... }
}
```

## Configuración de Base de Datos

### Estrategia de Migraciones

```bash
# Crear nueva migración
dotnet ef migrations add MigrationName -p WebFormula1.Infrastructure -s WebFormula1.Api

# Aplicar migraciones
dotnet ef database update

# Deshacer última migración
dotnet ef migrations remove
```

### Relaciones entre Entidades

```
Driver 1 ──── * DriverResult
 │ 1         * 
 │     ┌──────Race
 │     │
Team 1 ── * DriverResult

Driver 1 ──── * DriverStanding
Team 1 ──── * TeamStanding
Race 1 ──── * RaceNotification
```

## Trabajos Programados (Hangfire)

```csharp
// Sincronización diaria a las 2:00 AM
RecurringJob.AddOrUpdate<IFormula1SyncService>(
    "sync-current-season",
    service => service.SyncSeasonAsync(DateTime.Now.Year),
    Cron.Daily(2, 0));

// Envío de notificaciones cada hora
RecurringJob.AddOrUpdate<INotificationService>(
    "send-pending-notifications",
    service => service.SendPendingNotificationsAsync(),
    Cron.Hourly());
```

**Dashboard Hangfire**: `https://localhost:7001/hangfire`

## Manejo de Errores

### Estrategia Global

1. **Middleware de Excepción Global**: Captura todas las excepciones no manejadas
2. **Logging Centralizado**: Serilog registra todos los errores
3. **Respuestas de Error Consistentes**: Formato JSON estandarizado

```json
{
  "message": "Error description",
  "statusCode": 500,
  "timestamp": "2026-10-03T12:34:56Z"
}
```

## Seguridad

- Validación de entrada de datos
- HTTPS/TLS en producción
- CORS configurado
- Sanitización de datos
- Logging de intentos no autorizados

## Testing (Próximamente)

Estructura de pruebas:

```
tests/
├── WebFormula1.Core.Tests/
├── WebFormula1.Infrastructure.Tests/
├── WebFormula1.Api.Tests/
└── WebFormula1.Blazor.Tests/
```

## Escalabilidad

### Consideraciones para Producción

1. **Base de Datos**:
   - Usar Azure SQL Database
   - Implementar read replicas para consultas pesadas
   - Indexación y tuning de queries

2. **Caché**:
   - Redis para datos de clasificaciones
   - Caché de HTTP en navegadores

3. **CDN**:
   - Azure CDN para contenido estático

4. **Microservicios** (futuro):
   - Separar servicio de sincronización
   - Separar servicio de notificaciones
   - Message Queue (RabbitMQ/Azure Service Bus)

## Monitoreo y Observabilidad

- Application Insights para monitoreo
- Alertas en caso de fallos
- Dashboards de performance
- Trazas distribuidas

---

**Versión**: 1.0  
**Última actualización**: 2026-10-03
