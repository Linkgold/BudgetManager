# Budget Manager

## Descripción general

**Budget Manager** es una aplicación web de contabilidad personal diseñada para ayudar a los usuarios a llevar un control detallado de sus finanzas. Permite gestionar ingresos y gastos a través de un sistema de presupuestos mensuales, categorías personalizables y un seguimiento visual del estado financiero.

La aplicación está orientada a personas que buscan:

- **Organizar sus finanzas personales** de forma clara y sencilla.
- **Tomar decisiones informadas** sobre sus hábitos de gasto e ingreso.
- **Visualizar su evolución financiera** a través de resúmenes anuales y mensuales.
- **Mantener un control activo** de su economía sin necesidad de conocimientos contables avanzados.

### ¿Qué problema resuelve?

Muchas personas carecen de una herramienta accesible y visual para entender su situación financiera real. Budget Manager resuelve esta necesidad ofreciendo:

- Un **panel de control anual** que muestra el neto mensual y acumulado, permitiendo ver de un vistazo la evolución de las finanzas.
- La posibilidad de **clasificar ingresos y gastos por categorías**, con soporte para categorías mixtas que permiten flexibilidad.
- Un **sistema de presupuestos mensuales** que ayuda a planificar y comparar con la ejecución real.
- Una **vista detallada por mes** que permite analizar el desglose de cada categoría y gestionar transacciones individuales.

Con esta herramienta, el usuario pasa de tener una visión difusa de sus finanzas a contar con un **panorama claro y actualizado** que le permite **tomar el control de su economía**.


## Stack tecnológico

### Backend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **.NET** | 10.0 | Framework principal de la aplicación |
| **ASP.NET Core Web API** | 10.0 | Exposición de endpoints RESTful |
| **Entity Framework Core** | 10.0 | ORM para acceso a datos |
| **SQLite** | 3.53.3 | Base de datos embebida |
| **AutoMapper** | 16.2.0 | Mapeo entre entidades y DTOs |
| **FluentValidation** | 12.1.1 | Validación de DTOs de entrada |
| **BCrypt.Net-Next** | 4.2.0 | Hash de contraseñas |
| **JWT (JSON Web Tokens)** | 8.19.2 | Autenticación y autorización |

### Frontend

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **Blazor WebAssembly** | 10.0 | Framework de UI en el navegador |
| **MudBlazor** | 9.7.0 | Componentes UI y temas |

### Testing

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **xUnit** | 2.9.3 | Framework de pruebas unitarias |
| **Moq** | 4.20.72 | Creación de mocks en pruebas |
| **FluentAssertions** | 8.10.0 | Aserciones expresivas |
| **Microsoft.EntityFrameworkCore.InMemory** | 10.0.10 | Base de datos en memoria para pruebas |

### Patrones arquitectónicos

- **Domain-Driven Design (DDD)**
- **Patrón Repositorio**
- **Patrón Servicio**
- **Value Objects para datos inmutables**
- **Inyección de dependencias**

### Estructura de capas

```text
BudgetManager/
├── API/ # Capa de presentación (Controladores REST)
├── Application/ # Capa de aplicación (Servicios, DTOs, Validadores)
├── Domain/ # Capa de dominio (Entidades, Value Objects, Excepciones)
├── Infrastructure/ # Capa de infraestructura (Repositorios, DbContext)
├── Shared/ # DTOs compartidos entre API y UI
├── UI/ # Cliente Blazor WebAssembly
└── Tests/ # Pruebas unitarias y de integración


# Instalación y ejecución

## Requisitos previos

- .NET 10.0 SDK o superior
- Visual Studio 2026 o Visual Studio Code (opcional)
- Git (opcional, para clonar el repositorio)

## Clonar el repositorio

```text
git clone https://github.com/Linkgold/BudgetManager.git
cd BudgetManager
```

## Configuración

### Base de datos

La aplicación utiliza SQLite como base de datos embebida. No es necesario instalar ningún motor de base de datos adicional. La base de datos se crea automáticamente al ejecutar la aplicación por primera vez.

### Configuración de JWT

En el archivo `API/appsettings.json`, asegúrate de que la sección `Jwt` esté configurada correctamente:

```json
"Jwt": {
  "Key": "tu-clave-secreta-de-al-menos-32-caracteres",
  "Issuer": "BudgetAPI",
  "Audience": "BudgetClient",
  "ExpirationInMinutes": 15
}
```

### Configuración de la API

En el archivo `UI/appsettings.json`, verifica que la URL de la API sea correcta:

```json
"ApiUrl": "https://localhost:7131"
```

## Ejecutar la aplicación

### Desde Visual Studio

1. Abrir la solución `BudgetManager.sln`.
2. Seleccionar los proyecto `API` y UI como proyectos de inicio.
3. Presionar `F5` para ejecutar en modo depuración.
4. La API se ejecutará en `https://localhost:5280` (por defecto).
5. Abrir el proyecto `UI` en una instancia separada o configurar la ejecución simultánea.

### Desde la línea de comandos

#### Ejecutar la API

```text
cd API
dotnet run
```

#### Ejecutar la UI (Blazor WebAssembly)

```text
cd UI
dotnet run
```

## Acceso a la aplicación

| Componente | URL |
|---|---|
| UI (Blazor) | `https://localhost:7085` (por defecto) |
| API (Swagger) | `https://localhost:5280/swagger` |
| API (Endpoints) | `https://localhost:5280/api` |

## Primer acceso

1. Regístrate con un nuevo usuario en la página de registro: `/register`.
2. Inicia sesión con tus credenciales: `/login`.
3. Comienza a gestionar tus finanzas creando categorías, presupuestos, gastos fijos y transacciones.

## Ejecutar las pruebas

```text
cd Tests
dotnet test
```

## Notas adicionales

- La aplicación utiliza SQLite como base de datos, por lo que no requiere configuración de servidor de base de datos.
- El token JWT expira según el tiempo configurado en `ExpirationInMinutes`. Cuando expira, el usuario debe volver a iniciar sesión.
- La aplicación dispone de renovación automática de token cuando la sesión está próxima a expirar.
- Los datos se almacenan en el archivo `BudgetManager.db` dentro de la carpeta `API`.
- En el Index.html de la UI se puede ver un código comentado:

<!-- ✅ Content Security Policy (CSP)
<meta http-equiv="Content-Security-Policy" content="
    default-src 'self';
    script-src 'self' 'wasm-unsafe-eval';
    style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
    font-src 'self' https://fonts.gstatic.com;
    img-src 'self' data:;
    connect-src 'self' https://localhost:7131;
    object-src 'none';
    frame-src 'none';
    frame-ancestors 'none';
    base-uri 'self';
    form-action 'self';
    ">-->
	
	Éste código era para aplicar más seguridad y estar lo más cerca posible a OWASP, pero lamentablemente estaba dando muchos problemas con Azure, así que ahora mismo está deshabilitado.

# Estructuración

El proyecto está organizado siguiendo los principios de **Domain-Driven Design (DDD)** y **arquitectura por capas**, separando claramente las responsabilidades de cada módulo.

```text
BudgetManager/
│
├── API/                             # Capa de presentación
│   ├── Controllers/                 # Controladores REST
│   ├── Filters/                     # Filtros de acción (validación, token)
│   ├── Middleware/                  # Middleware de excepciones
│   ├── Program.cs                   # Punto de entrada
│   └── Startup.cs                   # Configuración de servicios
│
├── Application/                     # Capa de aplicación
│   ├── Interfaces/                  # Interfaces de servicios
│   ├── Services/                    # Implementación de servicios
│   ├── Mappings/                    # Configuración de AutoMapper
│   └── Validators/                  # Validadores FluentValidation
│
├── Domain/                          # Capa de dominio (núcleo)
│   ├── Entities/                    # Entidades de negocio (User, Category, Budget, etc.)
│   ├── ValueObjects/                # Value Objects (Money, EntityInfo, DailyPeriod, etc.)
│   ├── Exceptions/                  # Excepciones de dominio
│   ├── Enums/                       # Enumeraciones compartidas
│   └── Interfaces/                  # Interfaces de repositorios
│
├── Infrastructure/                 # Capa de infraestructura
│   ├── Data/                        # DbContext, Configuraciones, Factories
│   ├── Repositories/                # Implementación de repositorios
│   └── Extensions/                  # Extensiones de servicios
│
├── Shared/                          # DTOs compartidos
│   ├── DTOs/
│   │   ├── Request/                 # DTOs de petición
│   │   └── Response/                # DTOs de respuesta
│
├── UI/                              # Cliente Blazor WebAssembly
│   ├── Pages/                       # Páginas de la aplicación
│   ├── Shared/                      # Componentes compartidos
│   ├── Services/                    # Servicios de UI (API, Auth, Toast, etc.)
│   ├── Models/                      # Modelos de UI
│   ├── Helpers/                     # Helpers (Currency, Month, etc.)
│   ├── Extensions/                  # Extensiones y mappings
│   └── wwwroot/                     # Archivos estáticos (CSS, imágenes)
│
└── Tests/                           # Pruebas
    ├── API/                         # Pruebas de integración de la API
    ├── Application/                 # Pruebas unitarias de servicios
    ├── Domain/                      # Pruebas unitarias de entidades y Value Objects
    ├── Infrastructure/              # Pruebas de repositorios
    └── Helpers/                     # Helpers y factories para pruebas
```

## Flujo de datos

1. El usuario interactúa con la UI (Blazor).
2. La UI envía peticiones HTTP a la API.
3. Los Controladores reciben las peticiones y llaman a los Servicios de aplicación.
4. Los Servicios aplican la lógica de negocio y utilizan los Repositorios.
5. Los Repositorios acceden a la base de datos a través de Entity Framework Core.
6. La respuesta viaja en sentido inverso hasta la UI.

## Principios aplicados

- **Separación de responsabilidades:** Cada capa tiene una función específica y bien definida.
- **Inversión de dependencias:** Las capas superiores dependen de abstracciones (interfaces), no de implementaciones concretas.
- **Inmutabilidad:** Los Value Objects son inmutables, lo que garantiza su consistencia.
- **Encapsulamiento:** La lógica de negocio reside en las entidades y servicios de dominio.


# Funcionalidades

## Autenticación y gestión de usuarios

- Registro de nuevos usuarios con nombre, email y contraseña.
- Inicio de sesión seguro mediante JWT (JSON Web Tokens).
- Renovación automática del token cuando la sesión está próxima a expirar.
- Edición del perfil de usuario (nombre y email).
- Cambio de contraseña con verificación de la contraseña actual.
- Eliminación de cuenta con confirmación.

## Categorías

- Creación de categorías con nombre, descripción y naturaleza (Ingreso, Gasto o Mixta).
- Edición y eliminación de categorías.
- Filtrado por nombre, descripción y naturaleza.
- Validación para evitar la eliminación de categorías con dependencias (presupuestos o transacciones asociadas).

## Presupuestos

- Asignación de presupuestos mensuales por categoría.
- Creación en bloque para todo el año.
- Edición individual o en bloque.
- Eliminación de presupuestos por categoría y año.
- Visualización en tabla anual (categorías vs meses).
- Caché de datos para mejorar el rendimiento.

## Transacciones

- Registro de transacciones con concepto, importe, fecha, categoría y tipo (Ingreso/Gasto).
- Edición y eliminación de transacciones.
- Filtrado por categoría, tipo, mes y año.
- Visualización del total con signo (+/-).
- Modal reutilizable para crear, editar y eliminar transacciones (disponible tanto en la página de transacciones como en el detalle mensual).

## Gastos fijos

- Registro de gastos fijos recurrentes (mensuales) con nombre, importe, categoría y período.
- Edición y eliminación de gastos fijos.
- Filtrado por categoría y año.
- Solo permite categorías de tipo Gasto o Mixta.

## Dashboard

- Resumen anual con movimientos ejecutados vs presupuestos.
- Navegación por año mediante selector.
- Indicadores visuales (●) en los años con datos.
- Hover en columnas para resaltar el mes.
- Click en cualquier celda de mes para navegar a la vista detallada.
- Caché de datos para navegación instantánea.

## Detalle mensual (MonthDetail)

- Vista detallada de un mes específico.
- Resumen por categoría con presupuesto, ejecutado y diferencia.
- Expansión de categorías para ver transacciones individuales.
- Creación, edición y eliminación de transacciones desde la vista mensual.
- Tarjetas de resumen: Previsión neta, Real neto y Desviación.
- Navegación entre meses con flechas (◀ ▶).
- Caché de datos para navegación instantánea.

## Interfaz de usuario

- Modo oscuro y claro (persistente en el navegador).
- Notificaciones Toast (Success, Error y Warning).
- Componentes reutilizables (Modal, ConfirmModal, InfoBanner, TransactionModal).
- Diseño responsive para dispositivos móviles y escritorio.
- Barra de navegación con acceso rápido a todas las secciones.
- Indicadores visuales en filtros para años y meses con datos.

## Seguridad

- Autenticación basada en JWT.
- Hash de contraseñas con BCrypt.
- Autorización por roles (usuario autenticado).
- Validación de DTOs con FluentValidation.
- Manejo global de excepciones con middleware.
- Renovación automática de token.

## Rendimiento

- Caché de datos en el cliente (HasDataService).
- Caché de datos anuales (Dashboard y MonthDetail).
- Caché de datos por página (Transactions, Budgets, FixedExpenses).
- Consultas optimizadas con Entity Framework Core.
- Llamadas a la API con cancelación automática.