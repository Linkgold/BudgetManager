# Documentación de API REST - BudgetManager

## Descripción General

Esta es una API REST que proporciona operaciones CRUD completas para un sistema de gestión de presupuestos personales. Incluye endpoints para gestionar:

- **Categorías**: Clasificaciones para presupuestos y gastos
- **Presupuestos**: Límites mensuales por categoría
- **Gastos**: Transacciones individuales asociadas a presupuestos
- **Gastos Fijos**: Gastos recurrentes mensuales

---

## Endpoints

### 📂 Categorías (`/api/categories`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| GET | `/` | Obtiene todas las categorías | No |
| GET | `/{id}` | Obtiene una categoría por ID | No |
| POST | `/` | Crea una nueva categoría | No |
| PUT | `/{id}` | Actualiza una categoría | No |
| DELETE | `/{id}` | Desactiva una categoría | No |
| POST | `/{id}/activate` | Activa una categoría | No |

#### Ejemplos

**Crear Categoría**
```http
POST /api/categories
Content-Type: application/json

{
  "name": "Alimentación",
  "description": "Gastos de comida y bebidas"
}
```

**Respuesta:**
```json
{
  "id": 1,
  "name": "Alimentación",
  "description": "Gastos de comida y bebidas",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

---

### 💰 Presupuestos (`/api/budgets`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| GET | `/` | Obtiene todos los presupuestos | No |
| GET | `/{id}` | Obtiene un presupuesto por ID | No |
| GET | `/category/{categoryId}` | Obtiene presupuestos por categoría | No |
| POST | `/` | Crea un nuevo presupuesto | No |
| PUT | `/{id}` | Actualiza el monto de un presupuesto | No |
| DELETE | `/{id}` | Elimina un presupuesto | No |

#### Ejemplos

**Crear Presupuesto**
```http
POST /api/budgets
Content-Type: application/json

{
  "monthlyAmount": 500.00,
  "month": 1,
  "year": 2024,
  "categoryId": 1
}
```

**Respuesta:**
```json
{
  "id": 1,
  "monthlyAmount": 500.00,
  "month": 1,
  "year": 2024,
  "categoryId": 1,
  "categoryName": "Alimentación",
  "totalSpent": 0.00,
  "remaining": 500.00,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

---

### 🛒 Gastos (`/api/expenses`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| GET | `/` | Obtiene todos los gastos | No |
| GET | `/{id}` | Obtiene un gasto por ID | No |
| GET | `/budget/{budgetId}` | Obtiene gastos de un presupuesto | No |
| GET | `/by-period/{month}/{year}` | Obtiene gastos de un período | No |
| POST | `/` | Crea un nuevo gasto | No |
| DELETE | `/{id}` | Elimina un gasto | No |

#### Ejemplos

**Crear Gasto**
```http
POST /api/expenses
Content-Type: application/json

{
  "description": "Compra en supermercado",
  "amount": 45.50,
  "dateTime": "2024-01-15T14:30:00Z",
  "category": "Alimentación",
  "notes": "Compra semanal",
  "budgetId": 1
}
```

**Respuesta:**
```json
{
  "id": 1,
  "description": "Compra en supermercado",
  "amount": 45.50,
  "dateTime": "2024-01-15T14:30:00Z",
  "category": "Alimentación",
  "notes": "Compra semanal",
  "budgetId": 1,
  "createdAt": "2024-01-15T14:35:00Z"
}
```

---

### 📌 Gastos Fijos (`/api/fixedexpenses`)

| Método | Endpoint | Descripción | Autenticación |
|--------|----------|-------------|---------------|
| GET | `/` | Obtiene todos los gastos fijos | No |
| GET | `/{id}` | Obtiene un gasto fijo por ID | No |
| GET | `/active` | Obtiene gastos fijos activos | No |
| GET | `/category/{categoryId}` | Obtiene gastos fijos por categoría | No |
| POST | `/` | Crea un nuevo gasto fijo | No |
| PUT | `/{id}` | Actualiza un gasto fijo | No |
| DELETE | `/{id}` | Desactiva un gasto fijo | No |
| POST | `/{id}/activate` | Activa un gasto fijo | No |

#### Ejemplos

**Crear Gasto Fijo**
```http
POST /api/fixedexpenses
Content-Type: application/json

{
  "name": "Renta",
  "amount": 800.00,
  "chargeMonth": 1,
  "year": 2024,
  "chargeDay": 1,
  "description": "Alquiler del apartamento",
  "categoryId": null,
  "budgetId": null
}
```

**Respuesta:**
```json
{
  "id": 1,
  "name": "Renta",
  "amount": 800.00,
  "chargeMonth": 1,
  "year": 2024,
  "chargeDay": 1,
  "description": "Alquiler del apartamento",
  "isActive": true,
  "categoryId": null,
  "categoryName": null,
  "budgetId": null,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

---

## Códigos de Respuesta HTTP

| Código | Descripción |
|--------|-------------|
| 200 | OK - Solicitud exitosa |
| 201 | Created - Recurso creado exitosamente |
| 204 | No Content - Operación exitosa sin contenido |
| 400 | Bad Request - Datos inválidos o error de validación |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error - Error del servidor |

---

## Modelos de Datos

### CategoryDTO
```json
{
  "id": "int",
  "name": "string",
  "description": "string|null",
  "isActive": "boolean",
  "createdAt": "datetime",
  "updatedAt": "datetime|null"
}
```

### BudgetDTO
```json
{
  "id": "int",
  "monthlyAmount": "decimal",
  "month": "int (1-12)",
  "year": "int",
  "categoryId": "int",
  "categoryName": "string|null",
  "totalSpent": "decimal",
  "remaining": "decimal",
  "createdAt": "datetime",
  "updatedAt": "datetime|null"
}
```

### ExpenseDTO
```json
{
  "id": "int",
  "description": "string",
  "amount": "decimal",
  "dateTime": "datetime",
  "category": "string|null",
  "notes": "string|null",
  "budgetId": "int|null",
  "createdAt": "datetime"
}
```

### FixedExpenseDTO
```json
{
  "id": "int",
  "name": "string",
  "amount": "decimal",
  "chargeMonth": "int (1-12)",
  "year": "int",
  "chargeDay": "int|null",
  "description": "string|null",
  "isActive": "boolean",
  "categoryId": "int|null",
  "categoryName": "string|null",
  "budgetId": "int|null",
  "createdAt": "datetime",
  "updatedAt": "datetime|null"
}
```

---

## Validaciones

### Categorías
- `name`: Requerido, no puede estar vacío
- `description`: Opcional

### Presupuestos
- `monthlyAmount`: Debe ser > 0
- `month`: Debe estar entre 1 y 12
- `year`: Año válido
- `categoryId`: Debe existir

### Gastos
- `description`: Requerido, no puede estar vacío
- `amount`: Debe ser > 0
- `dateTime`: Fecha válida en formato ISO 8601
- `budgetId`: Opcional, pero si se proporciona debe coincidir el período

### Gastos Fijos
- `name`: Requerido, no puede estar vacío
- `amount`: Debe ser > 0
- `chargeMonth`: Debe estar entre 1 y 12
- `year`: Año válido
- `chargeDay`: Opcional (1-31)

---

## Notas Importantes

1. **Formatos de Fecha**: Todos los campos de fecha usan formato ISO 8601 en UTC (ej: `2024-01-15T10:30:00Z`)

2. **Soft Deletes**: Las categorías y gastos fijos usan soft delete (desactivación). Use el endpoint de activación para restaurarlos.

3. **Presupuestos Inmutables**: Una vez creado un presupuesto, solo se puede modificar el monto mensual.

4. **Gastos Inmutables**: Los gastos no pueden ser modificados una vez creados. Elimínelos y cree uno nuevo si es necesario.

5. **Asociaciones**: 
   - Los gastos pueden asociarse a presupuestos si coinciden el período
   - Los gastos fijos pueden asociarse a categorías y presupuestos

6. **Swagger UI**: Disponible en `/swagger` para pruebas interactivas de la API

---

## Ejemplos de Uso

### Flujo Completo

1. **Crear una categoría**
```bash
curl -X POST http://localhost:5000/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name":"Alimentación","description":"Gastos de comida"}'
```

2. **Crear un presupuesto**
```bash
curl -X POST http://localhost:5000/api/budgets \
  -H "Content-Type: application/json" \
  -d '{"monthlyAmount":500,"month":1,"year":2024,"categoryId":1}'
```

3. **Crear un gasto**
```bash
curl -X POST http://localhost:5000/api/expenses \
  -H "Content-Type: application/json" \
  -d '{"description":"Supermercado","amount":45.50,"dateTime":"2024-01-15T14:30:00Z","category":"Alimentación","budgetId":1}'
```

4. **Obtener presupuesto con gastos**
```bash
curl http://localhost:5000/api/budgets/1
curl http://localhost:5000/api/expenses/budget/1
```

---

## Configuración

La API se ejecuta en:
- **Desarrollo**: `https://localhost:5001` o `http://localhost:5000`
- **Base URL**: `{host}/api`

Todos los endpoints están bajo el prefijo `/api`.
