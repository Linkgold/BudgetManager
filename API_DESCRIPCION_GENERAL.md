# 📊 API REST - BudgetManager

## Descripción General

La **API REST de BudgetManager** es un backend completo construido con **ASP.NET Core 10** y **Entity Framework Core**, diseñado para gestionar presupuestos, categorías, gastos y gastos fijos. La API implementa una arquitectura de capas limpia con separación de responsabilidades entre las capas de controladores, servicios, aplicación, dominio e infraestructura.

### 🎯 Propósito

BudgetManager API proporciona endpoints REST seguros y documentados para:
- Gestionar categorías de gastos
- Crear y mantener presupuestos
- Registrar transacciones y gastos
- Administrar gastos fijos
- Autenticación y autorización de usuarios

---

## 🏗️ Arquitectura

El proyecto sigue una arquitectura de **4 capas**:

```
┌─────────────────────────────────────┐
│  API (Controllers)                  │  → Endpoints REST
├─────────────────────────────────────┤
│  Application (Services, DTOs)       │  → Lógica de negocio
├─────────────────────────────────────┤
│  Domain (Entities, ValueObjects)    │  → Modelos de dominio
├─────────────────────────────────────┤
│  Infrastructure (Repositories)      │  → Acceso a datos
└─────────────────────────────────────┘
```

### Proyectos de la Solución

| Proyecto | Descripción |
|----------|-------------|
| **API** | Controladores REST y configuración de ASP.NET Core |
| **Application** | DTOs, Interfaces de servicios y lógica de aplicación |
| **Domain** | Entidades, ValueObjects y reglas de negocio |
| **Infrastructure** | Repositorios, DbContext y acceso a datos (SQLite) |
| **Shared** | DTOs compartidas entre API y UI |
| **Contracts** | Interfaces de contrato para servicios |
| **UI** | Aplicación Blazor WebAssembly |
| **Tests** | Pruebas unitarias e integración |

---

## 📡 Endpoints Principales

### Categorías (`/api/categories`)
- **GET** `/api/categories` - Obtiene todas las categorías
- **GET** `/api/categories/{id}` - Obtiene una categoría por ID
- **POST** `/api/categories` - Crea una nueva categoría
- **PUT** `/api/categories/{id}` - Actualiza una categoría
- **DELETE** `/api/categories/{id}` - Elimina una categoría

### Presupuestos (`/api/budgets`)
- **GET** `/api/budgets` - Obtiene todos los presupuestos
- **GET** `/api/budgets/{id}` - Obtiene un presupuesto por ID
- **POST** `/api/budgets` - Crea un nuevo presupuesto
- **PUT** `/api/budgets/{id}/amount` - Actualiza el monto de un presupuesto
- **DELETE** `/api/budgets/{id}` - Elimina un presupuesto

### Gastos (`/api/expenses`)
- **GET** `/api/expenses` - Obtiene todos los gastos
- **GET** `/api/expenses/{id}` - Obtiene un gasto por ID
- **POST** `/api/expenses` - Crea un nuevo gasto
- **PUT** `/api/expenses/{id}` - Actualiza un gasto
- **DELETE** `/api/expenses/{id}` - Elimina un gasto

### Gastos Fijos (`/api/fixedexpenses`)
- **GET** `/api/fixedexpenses` - Obtiene todos los gastos fijos
- **GET** `/api/fixedexpenses/{id}` - Obtiene un gasto fijo por ID
- **POST** `/api/fixedexpenses` - Crea un nuevo gasto fijo
- **PUT** `/api/fixedexpenses/{id}` - Actualiza un gasto fijo
- **DELETE** `/api/fixedexpenses/{id}` - Elimina un gasto fijo

### Autenticación (`/api/auth`)
- **POST** `/api/auth/register` - Registra un nuevo usuario
- **POST** `/api/auth/login` - Autentica un usuario

---

## 🔐 Seguridad

### Autenticación
- **JWT (JSON Web Tokens)** para autenticación sin estado
- Tokens con expiración configurable
- Refresh tokens para renovación automática

### Autorización
- **Atributo `[Authorize]`** en todos los endpoints (excepto login/register)
- Control de acceso por usuario
- Validación de roles (si está implementado)

---

## 📦 Entidades de Dominio

### Category (Categoría)
```
- Id: Guid
- Name: string (requerido)
- Description: string
- IsActive: bool
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

### Budget (Presupuesto)
```
- Id: Guid
- CategoryId: Guid (FK)
- Amount: Money (ValueObject)
- Period: Period (ValueObject - Monthly, Yearly, etc.)
- StartDate: DateTime
- EndDate: DateTime?
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

### Expense (Gasto)
```
- Id: Guid
- BudgetId: Guid (FK)
- CategoryId: Guid (FK)
- Amount: Money (ValueObject)
- Description: string
- TransactionDate: DateTime
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

### FixedExpense (Gasto Fijo)
```
- Id: Guid
- CategoryId: Guid (FK)
- Amount: Money (ValueObject)
- Frequency: Frequency (Monthly, Quarterly, Annual)
- StartDate: DateTime
- EndDate: DateTime?
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

---

## 🔄 DTOs (Data Transfer Objects)

### Patrón de DTOs
- **{Entity}DTO**: Para lectura de datos (respuestas)
- **Create{Entity}DTO**: Para creación de recursos
- **Update{Entity}DTO**: Para actualización de recursos

### Ejemplo: CategoryDTO
```
{
  "id": "guid",
  "name": "Alimentación",
  "description": "Gastos de comida y supermercado",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

---

## 🗄️ Base de Datos

### Tecnología
- **SQLite** - Base de datos relacional ligera
- **Entity Framework Core** - ORM para acceso a datos
- **Migrations** - Versionado del esquema de base de datos

### Ubicación
```
Infrastructure/Data/Budget.db
```

---

## 🚀 Cómo Usar la API

### 1. Registrar un Usuario
```
POST /api/auth/register
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "TuPassword123!"
}
```

### 2. Autenticarse
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "TuPassword123!"
}
```

**Respuesta:**
```
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "..."
}
```

### 3. Usar el Token en Requests
```
GET /api/categories
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 4. Crear una Categoría
```
POST /api/categories
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Alimentación",
  "description": "Gastos de comida",
  "isActive": true
}
```

### 5. Crear un Presupuesto
```
POST /api/budgets
Authorization: Bearer {token}
Content-Type: application/json

{
  "categoryId": "guid",
  "amount": 500.00,
  "currency": "USD",
  "period": "Monthly",
  "startDate": "2024-01-01"
}
```

---

## 📋 Respuestas HTTP

### Códigos de Estado

| Código | Significado |
|--------|------------|
| **200** | OK - Solicitud exitosa |
| **201** | Created - Recurso creado exitosamente |
| **204** | No Content - Éxito sin contenido de respuesta |
| **400** | Bad Request - Datos inválidos |
| **401** | Unauthorized - Autenticación requerida |
| **403** | Forbidden - Autorización insuficiente |
| **404** | Not Found - Recurso no encontrado |
| **500** | Internal Server Error - Error del servidor |

### Formato de Respuesta de Error
```
{
  "status": 400,
  "message": "Validación fallida",
  "errors": {
	"name": ["El nombre es requerido"]
  }
}
```

---

## 🛠️ Stack Tecnológico

| Componente | Tecnología | Versión |
|-----------|-----------|---------|
| **Framework** | ASP.NET Core | 10.0 |
| **ORM** | Entity Framework Core | 10.0 |
| **Base de Datos** | SQLite | - |
| **Autenticación** | JWT | - |
| **Serialización** | System.Text.Json | - |
| **Lenguaje** | C# | 13.0 |

---

## 📝 Convenciones de Código

### Tipos Explícitos
- El proyecto utiliza **tipos explícitos** en lugar de `var`
- Mejora la legibilidad y mantenibilidad del código

### Comentarios XML
- Documentación con `/// <summary>`, `/// <param>`, `/// <returns>`
- Disponible en IntelliSense de Visual Studio

### Autorización
- Todos los controladores requieren el atributo `[Authorize]`
- Endpoints públicos deben explicitarse con `[AllowAnonymous]`

---

## 🔧 Configuración

### appsettings.json
```
{
  "ConnectionStrings": {
	"DefaultConnection": "Data Source=Budget.db"
  },
  "Jwt": {
	"SecretKey": "your-secret-key",
	"ExpiryInMinutes": 60,
	"RefreshTokenExpiryInDays": 7
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information"
	}
  }
}
```

---

## 📚 Recursos Adicionales

- [Documentación de ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [RESTful API Best Practices](https://restfulapi.net/)

---

## 👤 Autor
BudgetManager Team

## 📅 Última Actualización
Julio 2026
