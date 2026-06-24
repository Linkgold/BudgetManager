# 🚀 Guía Rápida - CRUD API

## Inicio Rápido

```bash
# Ejecutar la aplicación
dotnet run --project API

# Swagger UI estará disponible en:
https://localhost:5001/swagger
```

---

## 📋 Operaciones Básicas

### Crear una Categoría
```json
POST /api/categories
{
  "name": "Alimentación",
  "description": "Gastos de comida"
}
```

### Crear un Presupuesto
```json
POST /api/budgets
{
  "monthlyAmount": 500,
  "month": 1,
  "year": 2024,
  "categoryId": 1
}
```

### Crear un Gasto
```json
POST /api/expenses
{
  "description": "Supermercado",
  "amount": 45.50,
  "dateTime": "2024-01-15T14:30:00Z",
  "category": "Alimentación",
  "budgetId": 1
}
```

### Crear un Gasto Fijo
```json
POST /api/fixedexpenses
{
  "name": "Renta",
  "amount": 800,
  "chargeMonth": 1,
  "year": 2024,
  "chargeDay": 1,
  "categoryId": null
}
```

---

## 🔍 Consultas Útiles

```bash
# Obtener todas las categorías
GET /api/categories

# Obtener presupuestos del mes 1/2024
GET /api/budgets?month=1&year=2024

# Obtener gastos de enero 2024
GET /api/expenses/by-period/1/2024

# Obtener gastos fijos activos
GET /api/fixedexpenses/active

# Obtener presupuestos de una categoría
GET /api/budgets/category/1
```

---

## 🔄 Actualizaciones

```bash
# Actualizar categoría
PUT /api/categories/1
{
  "name": "Alimentación",
  "description": "Comida y bebidas"
}

# Actualizar monto de presupuesto
PUT /api/budgets/1
{
  "monthlyAmount": 600,
  "month": 1,
  "year": 2024,
  "categoryId": 1
}

# Actualizar gasto fijo
PUT /api/fixedexpenses/1
{
  "name": "Renta",
  "amount": 850,
  "chargeMonth": 1,
  "year": 2024,
  "chargeDay": 1
}
```

---

## 🗑️ Eliminaciones

```bash
# Desactivar categoría
DELETE /api/categories/1

# Eliminar presupuesto
DELETE /api/budgets/1

# Eliminar gasto
DELETE /api/expenses/1

# Desactivar gasto fijo
DELETE /api/fixedexpenses/1
```

---

## ✅ Reactivación

```bash
# Activar categoría
POST /api/categories/1/activate

# Activar gasto fijo
POST /api/fixedexpenses/1/activate
```

---

## 📊 Respuestas Esperadas

| Operación | Status | Cuerpo |
|-----------|--------|--------|
| CREATE exitoso | 201 | Objeto creado |
| READ exitoso | 200 | Lista u objeto |
| UPDATE exitoso | 204 | Vacío |
| DELETE exitoso | 204 | Vacío |
| Error validación | 400 | Mensaje de error |
| No encontrado | 404 | Mensaje de error |

---

## 🐛 Troubleshooting

**Error: Categoría no encontrada**
- Verificar que el CategoryId existe: `GET /api/categories/{id}`

**Error: Período de gasto no coincide**
- El gasto debe ser del mismo mes/año del presupuesto
- Ej: Presupuesto enero 2024 → Gasto debe ser en enero 2024

**Error: Presupuesto no encontrado**
- Verificar que el BudgetId existe: `GET /api/budgets/{id}`
- Crear presupuesto antes de asignar gastos

---

## 📚 Archivos Útiles

| Archivo | Propósito |
|---------|-----------|
| `API/API-ENDPOINTS.md` | Documentación completa |
| `API/test-requests.http` | Pruebas interactivas |
| `IMPLEMENTATION-SUMMARY.md` | Detalles técnicos |

---

## 🎯 Flujo Típico

1. Crear Categoría `POST /api/categories`
2. Crear Presupuesto `POST /api/budgets`
3. Crear Gastos `POST /api/expenses`
4. Consultar Presupuesto `GET /api/budgets/1`
5. Ver Remanente (calculado automáticamente)

---

## 💡 Tips

- Usa Swagger UI para una interfaz visual
- Copia ejemplos del archivo `test-requests.http`
- Prueba endpoints uno a uno
- Verifica que las referencias existan antes de crear
- Usa soft delete para auditoría (desactiva, no borra)

---

**¡Listo para empezar! 🚀**
