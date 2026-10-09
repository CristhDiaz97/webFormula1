# 🏎️ Web Fórmula 1 - Aplicación C#

Una aplicación completa de Fórmula 1 en C# que se integra con la API Jolpica F1, proporciona notificaciones automáticas y estadísticas detalladas de carreras y pilotos.

## 📋 Características

- ✅ Integración con API Jolpica F1
- ✅ Notificaciones automáticas 24 horas antes de eventos (Libres 1, 2, 3, Clasificación, Carrera)
- ✅ Horarios adaptados a zona horaria de Colombia (UTC-5)
- ✅ Estadísticas de carreras y pilotos
- ✅ Historia de escuderías
- ✅ Tabla de clasificación de pilotos
- ✅ Tabla de clasificación de constructores
- ✅ API RESTful
- ✅ Interfaz web con Blazor
- ✅ Trabajos programados con Hangfire
- ✅ Base de datos SQL Server

## 🏗️ Arquitectura

### Proyectos

1. **WebFormula1.Core** - Modelos de dominio y interfaces
2. **WebFormula1.Infrastructure** - Acceso a datos, integración con API, servicios
3. **WebFormula1.Api** - API ASP.NET Core
4. **WebFormula1.Blazor** - Interfaz web Blazor WebAssembly

### Base de Datos

Entidades principales:
- `Driver` - Pilotos
- `Team` - Escuderías
- `Race` - Carreras
- `DriverResult` - Resultados de pilotos en carreras
- `DriverStanding` - Clasificación de pilotos
- `TeamStanding` - Clasificación de constructores
- `RaceNotification` - Notificaciones programadas

## 🚀 Instalación y Configuración

### Requisitos Previos

- .NET 8.0 SDK
- SQL Server (LocalDB o versión completa)
- Visual Studio 2022 o Visual Studio Code

### Pasos de Instalación

1. **Clonar el repositorio**
```bash
cd C:\Dev\ProyectosIA\webFormula1
```

2. **Restaurar paquetes NuGet**
```bash
dotnet restore
```

3. **Configurar la base de datos**

Editar `src/WebFormula1.Api/appsettings.json` y configurar la conexión:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Formula1DB;Trusted_Connection=true;"
}
```

4. **Ejecutar migraciones de base de datos**
```bash
cd src/WebFormula1.Api
dotnet ef database update
```

5. **Configurar la URL de la API Jolpica F1**

Asegurarse de que en `appsettings.json`:
```json
"JolpicaF1": {
  "BaseUrl": "https://api.jolpica.com"
}
```

## 🏃 Ejecución

### Ejecutar la API

```bash
cd src/WebFormula1.Api
dotnet run
```

La API estará disponible en: `https://localhost:7001`

El Dashboard de Hangfire estará en: `https://localhost:7001/hangfire`

### Ejecutar Blazor (después de la API)

```bash
cd src/WebFormula1.Blazor
dotnet watch run
```

La aplicación web estará disponible en: `https://localhost:5001`

## 📡 API Endpoints

### Carreras
- `GET /api/races/{year}` - Obtener carreras de una temporada
- `GET /api/races/{year}/{round}` - Obtener detalles de una carrera
- `GET /api/races/upcoming` - Obtener próximas carreras

### Pilotos
- `GET /api/drivers/{year}` - Obtener pilotos de una temporada
- `GET /api/drivers/standings/{year}` - Obtener clasificación de pilotos
- `GET /api/drivers/{id}/details` - Obtener detalles de un piloto

### Escuderías
- `GET /api/teams` - Obtener todas las escuderías
- `GET /api/teams/standings/{year}` - Obtener clasificación de constructores
- `GET /api/teams/{id}/details` - Obtener detalles de una escudería
- `GET /api/teams/{id}/history` - Obtener historia de una escudería

## 🔔 Sistema de Notificaciones

### Características

- Notificaciones automáticas 24 horas antes de cada evento
- Eventos: Libres 1, Libres 2, Libres 3, Clasificación, Carrera
- Horarios en zona horaria de Colombia (COT)
- Trabajos programados con Hangfire

### Configuración

Los trabajos se configuran automáticamente en `Program.cs`:

- **Sincronización de datos**: Diariamente a las 2:00 AM
- **Envío de notificaciones**: Cada hora

## 🗄️ Base de Datos

### Diagrama de Relaciones

```
Driver
  ├── Team (FK)
  ├── DriverResult (1:N)
  └── DriverStanding (1:N)

Team
  ├── Drivers (1:N)
  └── TeamStanding (1:N)

Race
  └── DriverResult (1:N)

DriverResult
  ├── Driver (FK)
  └── Race (FK)

DriverStanding
  └── Driver (FK)

TeamStanding
  └── Team (FK)

RaceNotification
  └── Race (FK)
```

## 📊 Estadísticas Disponibles

### Estadísticas de Pilotos
- Campeonatos mundiales
- Carreras disputadas
- Puntos totales
- Victorias
- Podios
- Pole positions
- Vueltas rápidas

### Estadísticas de Escuderías
- Campeonatos constructores
- Mejor clasificación en carrera
- Pole positions
- Vueltas rápidas
- Historia y fundación

### Resultados de Carreras
- Posiciones finales
- Posición en parrilla
- Puntos obtenidos
- Vueltas completadas
- Tiempo total
- Estado final (Terminado, DNF, etc.)
- Vuelta rápida

## 🔐 Seguridad

- Validación de entrada de datos
- Manejo de excepciones centralizado
- Logging estructurado con Serilog
- Conexión HTTPS

## 📝 Logging

Los logs se almacenan en:
- Consola (durante desarrollo)
- Archivos: `logs/formula1-*.txt` (rotación diaria)

Niveles de log: Debug, Information, Warning, Error

## 🛠️ Tecnologías Utilizadas

- **Lenguaje**: C# .NET 8.0
- **Backend**: ASP.NET Core Web API
- **Frontend**: Blazor WebAssembly
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: SQL Server
- **Cliente HTTP**: Refit
- **Trabajos Programados**: Hangfire
- **Logging**: Serilog
- **Resilencia**: Polly
- **API Cliente**: Cliente HTTP fuertemente tipado

## 📚 Estructura de Carpetas

```
webFormula1/
├── src/
│   ├── WebFormula1.Core/
│   │   └── Models/
│   ├── WebFormula1.Infrastructure/
│   │   ├── Api/
│   │   ├── Data/
│   │   └── Services/
│   ├── WebFormula1.Api/
│   │   ├── Controllers/
│   │   └── appsettings.json
│   └── WebFormula1.Blazor/
│       ├── Pages/
│       ├── Components/
│       └── Services/
├── tests/
├── WebFormula1.sln
└── README.md
```

## 🐛 Troubleshooting

### Problema: "Database already exists or error creating database"

**Solución**: Eliminar la base de datos anterior y ejecutar las migraciones nuevamente:
```bash
dotnet ef database drop -f
dotnet ef database update
```

### Problema: "Connection timeout"

**Solución**: Verificar que SQL Server está ejecutándose y la cadena de conexión es correcta en `appsettings.json`

### Problema: "API key invalid"

**Solución**: Verificar que la URL base de Jolpica F1 API es correcta y que la API está disponible

## 📞 Soporte

Para reportar problemas o sugerencias, contactar al equipo de desarrollo.

## 📄 Licencia

Este proyecto es de código abierto bajo licencia MIT.

---

**Versión**: 1.0.0  
**Última actualización**: 2026-10-03  
**Desarrollador**: Equipo de Desarrollo
