# Sistema de Gestión de Gastos Personales

## Descripción General
Aplicación para el control y seguimiento de gastos personales que permite gestionar presupuestos mensuales, registrar gastos diarios y administrar gastos fijos anuales.

## Funcionalidades Principales

### Presupuestos Mensuales
- Creación de presupuestos por categorías (alimentación, ocio, transporte, etc.)
- Asignación de montos mensuales
- Seguimiento del cumplimiento vs gastos reales
- Control por mes y año

### Gastos
- Registro de gastos con fecha y hora
- Asignación automática al presupuesto correspondiente
- Clasificación por categorías
- Histórico de gastos

### Gastos Fijos Anuales
- Gestión de gastos recurrentes (suscripciones, seguros, servicios)
- Configuración del mes de cobro para cada gasto fijo
- Control por año (permite variaciones entre años)
- Estado activo/inactivo

### Control y Reportes
- Visualización del estado de los presupuestos (verde/amarillo/rojo)
- Resumen mensual de gastos
- Comparativa de gastos vs presupuesto asignado

## Tecnologías
- Backend: ASP.NET Core 8.0
- Base de Datos: SQL Server
- ORM: Entity Framework Core
- Documentación API: Swagger/OpenAPI

## Estructura de Datos
- **Presupuestos**: Tipo, monto mensual, mes, año
- **Gastos**: Descripción, monto, fecha/hora, categoría
- **Gastos Fijos**: Nombre, tipo, monto, mes cobro, año, día cobro

## Próximas Mejoras
- Autenticación de usuarios
- Múltiples usuarios
- Exportación a Excel/PDF
- Gráficos y visualizaciones
- App Android
