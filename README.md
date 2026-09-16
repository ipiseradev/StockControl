# StockControl

Sistema **multi-tenant** de control de stock e inventario para comercios, con actualizaciones en tiempo real y cobro de suscripción integrado con Mercado Pago.

Cada comercio (*tenant*) opera de forma completamente aislada: sus productos, depósitos, movimientos de stock y usuarios no son visibles ni accesibles para otros comercios registrados en la misma instancia.

## Arquitectura

El repositorio contiene dos proyectos independientes que conforman la solución completa:

| Proyecto | Descripción | Stack |
|---|---|---|
| [`StockControl.Api`](./StockControl.Api) | API REST + Hub de tiempo real | ASP.NET Core 10 (Minimal APIs), EF Core, PostgreSQL, SignalR |
| [`StockControl.Web`](./StockControl.Web) | Aplicación cliente | Blazor WebAssembly |

```
┌─────────────────────┐        HTTPS / REST         ┌──────────────────────┐
│   StockControl.Web   │ ───────────────────────────▶ │   StockControl.Api    │
│  (Blazor WASM, SPA)  │ ◀─────────────────────────── │  (Minimal API, JWT)   │
└─────────────────────┘        SignalR (WebSocket)    └──────────┬───────────┘
                                                                   │
                                                          EF Core  │  Npgsql
                                                                   ▼
                                                          ┌──────────────────┐
                                                          │   PostgreSQL     │
                                                          └──────────────────┘
```

## Funcionalidades

- **Autenticación y autorización** basada en JWT, con roles (`Administrador` / empleado) y aislamiento de datos por tenant en cada consulta.
- **Gestión de productos**: alta, edición, baja (con protección contra borrado si el producto tiene movimientos registrados), código de barras y SKU único.
- **Depósitos múltiples**: control de stock por depósito o mostrador.
- **Movimientos de stock**: entradas, salidas y ajustes, con historial completo por usuario y fecha.
- **Alertas de stock mínimo**: detección automática de productos por debajo del umbral configurado.
- **Tiempo real**: los cambios de stock se propagan al instante a todos los clientes conectados del mismo comercio vía SignalR.
- **Gestión de usuarios**: el administrador del comercio puede invitar y administrar empleados.
- **Suscripciones**: generación de links de pago recurrente con Mercado Pago y sincronización de estado vía webhook (validado contra la API de Mercado Pago, no confiando en el payload recibido).
- **Manejo de errores centralizado** mediante middleware propio.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para levantar PostgreSQL localmente) o una instancia de PostgreSQL propia
- Cuenta de [Mercado Pago Developers](https://www.mercadopago.com.ar/developers) (opcional, solo para probar el flujo de suscripciones)

## Puesta en marcha

### 1. Clonar el repositorio

```bash
git clone https://github.com/ipiseradev/StockControl.git
cd StockControl
```

### 2. Levantar la base de datos

```bash
cd StockControl.Api
docker compose up -d
```

Esto inicia PostgreSQL en el puerto `5433` con las credenciales por defecto definidas en `docker-compose.yml`.

### 3. Configurar secretos de la API

La API no versiona ningún secreto: `Jwt:Key` y `MercadoPago:AccessToken` se cargan desde [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (o variables de entorno en producción).

```bash
cd StockControl.Api
dotnet user-secrets set "Jwt:Key" "una-clave-secreta-larga-y-aleatoria"
dotnet user-secrets set "MercadoPago:AccessToken" "TEST-xxxxxxxx-xxxxxx"
```

### 4. Aplicar las migraciones

```bash
dotnet ef database update
```

### 5. Ejecutar la API

```bash
dotnet run
```

La API queda disponible en `http://localhost:5045` (ver `Properties/launchSettings.json`).

### 6. Ejecutar el cliente web

En otra terminal:

```bash
cd StockControl.Web
dotnet run
```

La aplicación queda disponible en `https://localhost:7127`.

> El primer usuario que se registra en `/auth/registro` crea automáticamente el comercio (tenant) y queda como su administrador.

## Estructura del proyecto

```
StockControl/
├── StockControl.Api/          # Backend (Minimal API)
│   ├── Data/                  # DbContext de EF Core
│   ├── Extensions/            # Extensiones (ClaimsPrincipal → tenant/usuario actual)
│   ├── Hubs/                  # Hub de SignalR para stock en tiempo real
│   ├── Middleware/            # Manejo centralizado de errores
│   ├── Migrations/            # Migraciones de EF Core
│   ├── Models/                # Entidades y DTOs
│   ├── Services/               # Servicios de dominio (JWT, Mercado Pago)
│   └── Program.cs             # Composición de la app y definición de endpoints
│
└── StockControl.Web/          # Frontend (Blazor WebAssembly)
    ├── Layout/                # Layouts de la aplicación
    ├── Pages/                 # Páginas (productos, depósitos, movimientos, usuarios, etc.)
    ├── Services/               # Clientes HTTP y de SignalR hacia la API
    └── Program.cs              # Configuración de servicios del cliente
```

## Modelo de dominio

- **Tenant**: comercio dado de alta, con su propio estado de suscripción.
- **Usuario**: pertenece a un tenant, con rol `Administrador` o empleado.
- **Producto**: pertenece a un tenant; el stock actual se calcula a partir de sus movimientos.
- **Deposito**: ubicación física (o mostrador) donde se registra stock.
- **MovimientoStock**: entrada, salida o ajuste de stock de un producto en un depósito, asociado al usuario que lo generó.

## Seguridad

- Contraseñas hasheadas con BCrypt.
- Autenticación mediante JWT firmado, validado en cada request.
- Todas las consultas filtran explícitamente por `TenantId` para evitar fugas de datos entre comercios.
- El webhook de Mercado Pago no confía en el estado recibido en el payload: siempre confirma el estado real contra la API de Mercado Pago antes de actualizar la suscripción.

## Roadmap

- [ ] Tests automatizados (unitarios e integración)
- [ ] Pipeline de CI/CD
- [ ] Contenerización de la API con Docker
- [ ] Reportes y exportación de movimientos

## Licencia

Este proyecto es privado / propietario. Todos los derechos reservados.
