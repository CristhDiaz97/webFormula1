# 🚀 Guía de Instalación - Web Fórmula 1

## Requisitos Previos

Asegúrate de tener instalado:
- **.NET 8.0 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
- **SQL Server** (LocalDB incluido con Visual Studio) o SQL Server Community Edition
- **Visual Studio 2022** (Community, Professional) o **VS Code** con C# Dev Kit
- **Git** (opcional, para control de versiones)

## Paso 1: Verificar Instalación de .NET

Abre PowerShell y ejecuta:

```powershell
dotnet --version
```

Deberías ver: `8.0.x` o superior

## Paso 2: Restaurar Paquetes NuGet

Navega a la carpeta del proyecto:

```powershell
cd C:\Dev\ProyectosIA\webFormula1
dotnet restore
```

Esto descargará todas las dependencias necesarias.

## Paso 3: Configurar la Base de Datos

### Opción A: Usar LocalDB (Recomendado para desarrollo)

LocalDB viene con Visual Studio. Verifica que esté disponible:

```powershell
sqllocaldb info mssqllocaldb
```

Si no está disponible, instálalo:

```powershell
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb
```

### Opción B: Usar SQL Server Express

Descarga de: https://www.microsoft.com/sql-server/sql-server-express

## Paso 4: Crear Migraciones de Base de Datos

Navega a la carpeta del proyecto API:

```powershell
cd src\WebFormula1.Api
```

Crea la migración inicial:

```powershell
dotnet ef migrations add InitialCreate --project ..\WebFormula1.Infrastructure --startup-project .
```

Esto creará la carpeta `Migrations/` con los archivos necesarios.

## Paso 5: Aplicar las Migraciones

Ejecuta la base de datos:

```powershell
dotnet ef database update
```

Deberías ver un mensaje confirmando que la base de datos se creó exitosamente.

### Verificar la Base de Datos

Abre SQL Server Management Studio o Azure Data Studio y conecta a:
- **Servidor**: `(localdb)\mssqllocaldb`
- **Base de datos**: `Formula1DB`

Deberías ver las tablas creadas.

## Paso 6: Configurar URL de API Jolpica F1

Edita `src/WebFormula1.Api/appsettings.json`:

```json
"JolpicaF1": {
  "BaseUrl": "https://api.jolpica.com"
}
```

**Importante**: Verifica la URL correcta en: https://www.jolpica.com/docs

## Paso 7: Ejecutar la API

Desde la carpeta `src/WebFormula1.Api`:

```powershell
dotnet run
```

Deberías ver:

```
Web Fórmula 1 API starting...
Now listening on: https://localhost:7001
Application started. Press Ctrl+C to shut down.
```

### Acceder a la API

- **Swagger UI** (documentación interactiva): https://localhost:7001/swagger
- **Hangfire Dashboard** (trabajos programados): https://localhost:7001/hangfire

## Paso 8: Ejecutar Blazor (Aplicación Web)

Abre una nueva ventana de PowerShell y navega a:

```powershell
cd C:\Dev\ProyectosIA\webFormula1\src\WebFormula1.Blazor
```

Ejecuta con watch mode (recargará automáticamente en cambios):

```powershell
dotnet watch run
```

O simplemente:

```powershell
dotnet run
```

La aplicación estará disponible en: https://localhost:5001

## Primer Test: Obtener Datos

1. Abre https://localhost:7001/swagger en tu navegador
2. Haz clic en un endpoint, ej: `GET /api/races/2026`
3. Presiona "Try it out"
4. Presiona "Execute"

Deberías obtener un JSON vacío `[]` inicialmente (sin datos sincronizados aún).

## Sincronizar Datos

Hay dos opciones:

### Opción 1: Esperar el trabajo programado (2:00 AM)

El trabajo automático ejecuta: `SyncSeasonAsync(2026)` diariamente a las 2:00 AM.

### Opción 2: Sincronizar Manualmente

Crea un endpoint temporal en `RacesController.cs`:

```csharp
[HttpPost("sync/{year}")]
public async Task<IActionResult> SyncSeason(int year)
{
    var service = HttpContext.RequestServices.GetRequiredService<IFormula1SyncService>();
    await service.SyncSeasonAsync(year);
    return Ok("Sincronización completada");
}
```

Luego en Swagger: `POST /api/races/sync/2026`

## Solucionar Problemas

### Error: "Database already exists"

Elimina y recrea la base de datos:

```powershell
dotnet ef database drop -f --project ..\WebFormula1.Infrastructure
dotnet ef database update --project ..\WebFormula1.Infrastructure
```

### Error: "Connection timeout"

Verifica que SQL Server está ejecutándose:

```powershell
sqllocaldb start mssqllocaldb
```

### Error: "The StartUp project could not be determined"

Especifica el startup project:

```powershell
dotnet ef migrations add InitialCreate --startup-project . --project ..\WebFormula1.Infrastructure
```

### Error: "Could not resolve service for type"

Asegúrate de que todos los servicios están registrados en `Program.cs`.

### Error: "API de Jolpica F1 no responde"

Verifica la URL en `appsettings.json` y que tienes conexión a internet.

## Estructura de Carpetas de Salida

```
WebFormula1/
├── bin/                    # Archivos compilados
├── obj/                    # Archivos temporales
├── Migrations/             # Migraciones de EF Core
├── logs/                   # Archivos de log
│   └── formula1-*.txt
└── Properties/
    └── launchSettings.json # Configuración de ejecución
```

## Próximas Pruebas

Después de que se ejecute correctamente:

1. ✅ Sincronizar datos de F1
2. ✅ Verificar datos en la BD
3. ✅ Consultar datos vía API
4. ✅ Visualizar en Blazor
5. ✅ Probar notificaciones

## 📚 Documentación Adicional

- [README.md](README.md) - Visión general del proyecto
- [ARCHITECTURE.md](ARCHITECTURE.md) - Arquitectura detallada
- [NEXT_STEPS.md](NEXT_STEPS.md) - Próximas fases de desarrollo

## 🆘 Soporte

Si encuentras problemas:

1. Revisa los logs en `logs/formula1-*.txt`
2. Verifica la consola de salida (errores de EF Core)
3. Asegúrate de que todos los requisitos previos están instalados
4. Intenta limpiar y restaurar: `dotnet clean && dotnet restore`

---

**Última actualización**: 2026-10-03
