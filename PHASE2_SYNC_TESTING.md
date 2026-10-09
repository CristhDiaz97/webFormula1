# 🔄 Fase 2: Testing de Sincronización con Jolpica F1 API

## 🚀 Iniciar la Aplicación

### Paso 1: Abrir PowerShell y navegar a la carpeta API

```powershell
cd C:\Dev\ProyectosIA\webFormula1\src\WebFormula1.Api
dotnet run
```

### Paso 2: Esperar a que inicie

Deberías ver algo como:

```
Web Fórmula 1 API starting...
Now listening on: https://localhost:7001
Application started. Press Ctrl+C to shut down.
```

---

## 📡 Testing de API - Endpoints Disponibles

### 1️⃣ Verificar Estado del Sistema

**URL**: `GET https://localhost:7001/api/apistatus/status`

**Respuesta esperada**:
```json
{
  "timestamp": "2026-10-07T20:30:00Z",
  "version": "1.0.0",
  "endpoints": {
    "health": "/api/apistatus/health",
    "seasons": "/api/apistatus/seasons",
    "sync": {...},
    "data": {...}
  }
}
```

---

### 2️⃣ Verificar Salud de API Jolpica F1

**URL**: `GET https://localhost:7001/api/apistatus/health`

**Respuesta si la API está disponible**:
```json
{
  "isHealthy": true,
  "message": "API Jolpica F1 está funcionando correctamente",
  "availableSeasons": 75
}
```

**Respuesta si la API NO está disponible**:
```json
{
  "isHealthy": false,
  "message": "Error de conexión: ...",
  "availableSeasons": 0
}
```

---

### 3️⃣ Obtener Temporadas Disponibles

**URL**: `GET https://localhost:7001/api/apistatus/seasons`

**Respuesta esperada**:
```json
{
  "seasons": [2026, 2025, 2024, 2023, ...],
  "count": 75
}
```

---

### 4️⃣ Sincronizar Datos de una Temporada Completa

**URL**: `POST https://localhost:7001/api/sync/season/2026`

**Respuesta**:
```json
{
  "message": "Sincronización de temporada 2026 completada exitosamente"
}
```

---

### 5️⃣ Sincronizar Solo Carreras

**URL**: `POST https://localhost:7001/api/sync/races/2026`

**Respuesta**:
```json
{
  "message": "Sincronización de carreras 2026 completada"
}
```

---

### 6️⃣ Sincronizar Pilotos

**URL**: `POST https://localhost:7001/api/sync/drivers/2026`

**Respuesta**:
```json
{
  "message": "Sincronización de pilotos 2026 completada"
}
```

---

### 7️⃣ Sincronizar Equipos

**URL**: `POST https://localhost:7001/api/sync/teams/2026`

**Respuesta**:
```json
{
  "message": "Sincronización de equipos 2026 completada"
}
```

---

### 8️⃣ Sincronizar Clasificaciones

**URL**: `POST https://localhost:7001/api/sync/standings/2026`

**Respuesta**:
```json
{
  "message": "Sincronización de clasificaciones 2026 completada"
}
```

---

### 9️⃣ Sincronizar Detalles de una Carrera Específica

**URL**: `POST https://localhost:7001/api/sync/race/2026/1`

**Respuesta**:
```json
{
  "message": "Sincronización de detalles carrera 2026/1 completada"
}
```

---

### 🔟 Ver Datos Sincronizados

**Carreras**:
```
GET https://localhost:7001/api/races/2026
```

**Drivers**:
```
GET https://localhost:7001/api/drivers/standings/2026
```

**Teams**:
```
GET https://localhost:7001/api/teams/standings/2026
```

---

## 🧪 Plan de Testing Recomendado

### Prueba 1: Verificar Conectividad (5 min)

1. Abre Swagger: `https://localhost:7001/swagger`
2. Ve a **ApiStatus**
3. Prueba el endpoint `/api/apistatus/health`
4. Verifica que la API Jolpica F1 está disponible

### Prueba 2: Ver Temporadas Disponibles (5 min)

1. En Swagger, ve a **ApiStatus**
2. Prueba `/api/apistatus/seasons`
3. Anota el año de una temporada (ej: 2026)

### Prueba 3: Sincronizar Temporada 2026 (10-30 min)

1. Ve a **Sync**
2. Prueba `POST /api/sync/season/2026`
3. Espera a que complete (puede tomar varios minutos)
4. Verifica en SQL Server que se cargaron datos

### Prueba 4: Consultar Datos Sincronizados (5 min)

1. Ve a **Races**
2. Prueba `GET /api/races/2026`
3. Deberías ver las carreras de 2026

---

## 📊 Verificar Datos en Base de Datos

### Conéctate con SQL Server Management Studio

```sql
-- Ver todas las carreras sincronizadas
SELECT * FROM Races WHERE SeasonYear = 2026;

-- Ver cantidad de pilotos
SELECT COUNT(*) as TotalDrivers FROM Drivers;

-- Ver cantidad de equipos
SELECT COUNT(*) as TotalTeams FROM Teams;

-- Ver último sync
SELECT TOP 1 * FROM __EFMigrationsHistory ORDER BY InstalledOn DESC;
```

---

## ⚠️ Posibles Errores y Soluciones

### Error: "isHealthy": false

**Causa**: La API Jolpica F1 no está disponible o la URL es incorrecta

**Solución**:
1. Verifica la URL en `appsettings.json`: `"BaseUrl": "https://api.jolpica.com"`
2. Prueba manualmente en navegador
3. Verifica tu conexión a internet

### Error: "A connection was successfully established..."

**Causa**: Problema de SSL/Certificado

**Solución**: Ya está solucionado con `TrustServerCertificate=true` en la cadena de conexión

### Error: "Connection timeout"

**Causa**: Base de datos no responde o SQL Server Express no está ejecutándose

**Solución**:
1. Verifica que SQL Server Express esté ejecutándose
2. Reinicia el servicio: `sqllocaldb stop mssqllocaldb && sqllocaldb start mssqllocaldb`

---

## 🔍 Monitorear Sincronización

Abre la consola donde ejecutas `dotnet run` para ver logs en tiempo real:

```
[20:35:14 INF] Iniciando sincronización de temporada 2026
[20:35:15 INF] Sincronizando carreras...
[20:35:20 INF] Carreras sincronizadas para año 2026
[20:35:21 INF] Sincronizando pilotos...
[20:35:30 INF] Pilotos sincronizados para año 2026
...
```

---

## ✅ Checklist de Validación

- [ ] API inicia sin errores (`dotnet run`)
- [ ] `/api/apistatus/health` retorna `isHealthy: true`
- [ ] `/api/apistatus/seasons` lista temporadas disponibles
- [ ] `POST /api/sync/season/2026` se ejecuta sin errores
- [ ] `GET /api/races/2026` retorna carreras
- [ ] BD tiene datos en tabla `Races`
- [ ] BD tiene datos en tabla `Drivers`
- [ ] BD tiene datos en tabla `Teams`
- [ ] Logs muestran sincronización exitosa

---

## 🎯 Próximos Pasos

1. ✅ Ejecutar la API
2. ✅ Verificar conectividad con Jolpica F1
3. ✅ Sincronizar datos de 2026
4. 📋 Validar datos en base de datos
5. 📋 Configurar notificaciones automáticas
6. 📋 Crear interfaz Blazor para visualizar datos

---

**Fecha**: 07 de Octubre de 2026  
**Estado**: Fase 2 - Testing de Sincronización
