# 🎨 Interfaz de Usuario - BudgetManager

## Descripción General

La **UI de BudgetManager** es una aplicación web moderna construida con **Blazor WebAssembly** (.NET 10), proporcionando una interfaz intuitiva y responsiva para gestionar presupuestos, categorías, gastos y transacciones en tiempo real. La interfaz incluye soporte para temas claros y oscuros, y está diseñada para ser accesible y eficiente.

### 🎯 Características Principales

- ✅ Interfaz responsiva (Desktop, Tablet, Mobile)
- ✅ Autenticación y autorización de usuarios
- ✅ Panel de control (Dashboard) con gráficos e indicadores
- ✅ Gestión completa de presupuestos, categorías y gastos
- ✅ Tema claro/oscuro switcheable
- ✅ Notificaciones visual (Toast)
- ✅ Formularios con validación en cliente

---

## 🏗️ Arquitectura

### Tipo de Aplicación
- **Blazor WebAssembly** - Ejecución en navegador con C# compilado a WebAssembly
- **SPA (Single Page Application)** - Navegación sin recarga de página
- **.NET 10** - Framework más reciente

### Estructura de Carpetas

```
UI/
├── Pages/                      # Páginas Razor (.razor)
│   ├── Index.razor            # Página de inicio
│   ├── Login.razor            # Autenticación
│   ├── Register.razor         # Registro de usuarios
│   ├── Dashboard.razor        # Panel de control
│   ├── Budgets.razor          # Gestión de presupuestos
│   ├── Categories.razor       # Gestión de categorías
│   ├── Expenses.razor         # Registro de gastos
│   ├── Transactions.razor     # Historial de transacciones
│   └── FixedExpenses.razor    # Gastos fijos
│
├── Shared/                     # Componentes compartidos
│   ├── MainLayout.razor       # Layout principal
│   ├── NavMenu.razor          # Menú de navegación
│   ├── BasePage.cs            # Clase base para páginas
│   ├── ToastContainer.razor   # Contenedor de notificaciones
│   └── Components/            # Componentes reutilizables
│
├── wwwroot/                    # Contenido estático
│   ├── css/                    # Estilos CSS
│   │   ├── app.css            # Estilos globales
│   │   ├── base/              # Estilos base (reset, tipografía)
│   │   ├── components/        # Estilos de componentes (buttons, forms)
│   │   ├── pages/             # Estilos específicos de páginas
│   │   └── themes/            # Temas (light, dark)
│   ├── js/                     # Scripts JavaScript
│   ├── images/                # Imágenes
│   └── icon.png               # Favicon
│
├── Services/                   # Servicios de la aplicación
│   ├── AuthService.cs         # Autenticación y autorización
│   ├── HttpClientService.cs   # Comunicación con API
│   ├── ThemeService.cs        # Gestión de temas
│   └── ToastService.cs        # Sistema de notificaciones
│
├── App.razor                  # Componente raíz
├── _Imports.razor             # Importaciones globales
└── Program.cs                 # Configuración de arranque
```

---

## 📄 Páginas Principales

### 1. **Index.razor** - Página de Inicio
- Primera página que ve el usuario no autenticado
- Enlace de bienvenida a Login/Register
- Información general sobre BudgetManager

### 2. **Login.razor** - Autenticación
```
Campos:
  - Email (requerido)
  - Contraseña (requerida)

Funcionalidad:
  - Validación de credenciales
  - Almacenamiento de JWT token
  - Redirección a Dashboard tras login exitoso
```

### 3. **Register.razor** - Registro de Usuarios
```
Campos:
  - Email (requerido, único)
  - Contraseña (requerida, validación fuerte)
  - Confirmar Contraseña (debe coincidir)

Funcionalidad:
  - Validación de formulario
  - Creación de nueva cuenta
  - Redirección a Login tras registro
```

### 4. **Dashboard.razor** - Panel de Control
```
Secciones:
  - Resumen de presupuestos
  - Gráficos de gastos vs presupuestos
  - Indicadores de alertas de presupuestos excedidos
  - Actividad reciente
  - Gastos fijos próximos

Características:
  - Gráficos interactivos (Chart.js)
  - Actualización en tiempo real
  - Indicadores visuales de estado
```

### 5. **Budgets.razor** - Gestión de Presupuestos
```
Vista de Tabla:
  - Listar todos los presupuestos por categoría
  - Mostrar monto, período y estado (On Track/Over Budget)

Modal de Creación:
  - Seleccionar categoría
  - Ingresar monto
  - Seleccionar período (Monthly, Yearly, etc.)
  - Establecer fechas de inicio/fin

Acciones:
  - Crear nuevo presupuesto
  - Editar presupuesto existente
  - Eliminar presupuesto
  - Ver detalles con gráfico de gastos
```

### 6. **Categories.razor** - Gestión de Categorías
```
Vista de Tabla:
  - Listar todas las categorías
  - Mostrar nombre, descripción y estado

Modal de Creación/Edición:
  - Nombre (requerido)
  - Descripción
  - Estado activo/inactivo

Acciones:
  - Crear categoría
  - Editar categoría
  - Eliminar categoría
  - Activar/Desactivar categoría
```

### 7. **Expenses.razor** - Registro de Gastos
```
Vista de Tabla:
  - Listar gastos registrados
  - Mostrar categoría, monto, fecha y descripción
  - Filtrar por fecha/categoría

Modal de Creación:
  - Seleccionar presupuesto
  - Seleccionar categoría
  - Ingresar monto
  - Descripción (opcional)
  - Fecha de transacción

Acciones:
  - Registrar nuevo gasto
  - Editar gasto
  - Eliminar gasto
  - Validación: monto no debe exceder presupuesto disponible
```

### 8. **Transactions.razor** - Historial de Transacciones
```
Vista:
  - Listar todas las transacciones (gastos y gastos fijos)
  - Filtros avanzados (fecha, categoría, monto rango)
  - Búsqueda por descripción

Características:
  - Paginación
  - Ordenamiento por columnas
  - Exportar a CSV/Excel (opcional)
  - Gráficos de tendencias
```

### 9. **FixedExpenses.razor** - Gastos Fijos
```
Vista de Tabla:
  - Listar gastos fijos
  - Mostrar categoría, monto, frecuencia

Modal de Creación:
  - Seleccionar categoría
  - Ingresar monto
  - Seleccionar frecuencia (Monthly, Quarterly, Annual)
  - Establecer fechas de vigencia

Acciones:
  - Crear gasto fijo
  - Editar gasto fijo
  - Eliminar gasto fijo
```

---

## 🧩 Componentes Compartidos

### NavMenu.razor
```
Menú de navegación lateral que incluye:
  - Logo de aplicación
  - Enlaces a páginas principales (Dashboard, Presupuestos, etc.)
  - Información del usuario logueado
  - Botón de logout
  - Selector de tema (Light/Dark)
```

### ToastContainer.razor
```
Sistema de notificaciones flotantes:
  - Toast de éxito (operación completada)
  - Toast de error (operación fallida)
  - Toast de aviso (información importante)
  - Auto-cierre tras 3-5 segundos
```

### BasePage.cs
```
Clase base con funcionalidad compartida:
  - Acceso a servicios (AuthService, HttpClientService, etc.)
  - Métodos comunes (mostrar notificaciones, validar autorización)
  - Gestión del ciclo de vida del componente
  - Protected virtual methods para override en páginas hijas
```

---

## 🎨 Estilos y Temas

### Arquitectura CSS Modular

```
css/
├── app.css              # Índice principal (importa otros)
│
├── base/
│   ├── reset.css        # Reset de estilos navegador
│   ├── variables.css    # Variables CSS (colores, tamaños, tipografía)
│   ├── typography.css   # Tipografía global
│   └── utilities.css    # Clases de utilidad (spacing, display, etc.)
│
├── components/
│   ├── buttons.css      # Estilos de botones
│   ├── forms.css        # Estilos de formularios (inputs, selects)
│   ├── cards.css        # Estilos de tarjetas
│   ├── modals.css       # Estilos de modales
│   ├── tables.css       # Estilos de tablas
│   ├── navbar.css       # Barra de navegación
│   └── toasts.css       # Notificaciones toast
│
├── pages/
│   ├── dashboard.css    # Estilos página Dashboard
│   ├── budgets.css      # Estilos página Presupuestos
│   ├── categories.css   # Estilos página Categorías
│   ├── expenses.css     # Estilos página Gastos
│   └── transactions.css # Estilos página Transacciones
│
└── themes/
	├── light-theme.css  # Variables tema claro
	└── dark-theme.css   # Variables tema oscuro
```

### Variables CSS Globales

```
:root {
  /* Colores primarios */
  --primary-color: #007bff;
  --secondary-color: #6c757d;
  --success-color: #28a745;
  --warning-color: #ffc107;
  --danger-color: #dc3545;

  /* Neutral */
  --background-color: #ffffff;
  --text-color: #212529;
  --border-color: #dee2e6;

  /* Spacing */
  --spacing-unit: 0.5rem;
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 2rem;

  /* Tipografía */
  --font-family-base: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto;
  --font-size-base: 1rem;
  --font-weight-normal: 400;
  --font-weight-bold: 700;
}
```

### Temas

#### Light Theme (Por defecto)
```
/* Fondo claro, texto oscuro */
--background-color: #ffffff;
--text-color: #212529;
--card-background: #f8f9fa;
```

#### Dark Theme
```
/* Fondo oscuro, texto claro */
--background-color: #1a1a1a;
--text-color: #e0e0e0;
--card-background: #2d2d2d;
```

---

## 🔐 Autenticación y Autorización

### Flujo de Autenticación

```
Usuario no autenticado
		↓
	[Login/Register]
		↓
	API (RecibeDatos)
		↓
	Validación exitosa → Retorna JWT Token
		↓
	[Guardar Token en Local Storage]
		↓
	[Redirect a Dashboard]
		↓
	Todas las peticiones incluyen: "Authorization: Bearer {token}"
```

### AuthService.cs
```
Funcionalidad:
  - Register(email, password): Task<RegistrationResponse>
  - Login(email, password): Task<LoginResponse>
  - Logout(): void
  - GetToken(): string?
  - IsAuthenticated(): bool
  - RefreshToken(): Task<bool>

Almacenamiento:
  - JWT token → LocalStorage
  - Refresh token → LocalStorage (opcional)
```

### Protección de Rutas

```
<!-- Ruta pública (disponible para todos) -->
@page "/login"

<!-- Ruta protegida (solo usuarios autenticados) -->
@page "/budgets"
@attribute [Authorize]

<!-- Ruta con rol específico -->
@page "/admin"
@attribute [Authorize(Roles = "Admin")]
```

---

## 🌐 Comunicación con API

### HttpClientService.cs

```
Métodos principales:
  - GetAsync<T>(url, headers): Task<T>
  - PostAsync<T>(url, data, headers): Task<T>
  - PutAsync<T>(url, data, headers): Task<T>
  - DeleteAsync(url, headers): Task<bool>

Características:
  - Incluye automáticamente JWT en Authorization header
  - Maneja errores HTTP comunes
  - Refresca token si expira (401)
  - Serializa/Deserializa JSON
```

### Ejemplo de Petición

```
// Obtener todas las categorías
HttpClientService http = new HttpClientService(httpClient, authService);
List<CategoryDTO> categories = await http.GetAsync<List<CategoryDTO>>(
	"/api/categories"
);
```

---

## 🎛️ Servicios Principales

### 1. **AuthService**
```
Responsabilidades:
  - Autenticación de usuarios
  - Gestión de tokens JWT
  - Validación de autorización
```

### 2. **HttpClientService**
```
Responsabilidades:
  - Comunicación HTTP con API
  - Serialización/Deserialización
  - Manejo de errores
```

### 3. **ThemeService**
```
Responsabilidades:
  - Cambiar tema claro/oscuro
  - Guardar preferencia en LocalStorage
  - Notificar componentes de cambios
```

### 4. **ToastService**
```
Responsabilidades:
  - Mostrar notificaciones
  - Gestionar cola de toasts
  - Auto-cerrar notificaciones
```

---

## 📱 Responsividad

### Breakpoints de Bootstrap

| Breakpoint | Tamaño | Dispositivo |
|-----------|--------|-----------|
| XS | < 576px | Móvil pequeño |
| SM | ≥ 576px | Móvil |
| MD | ≥ 768px | Tablet |
| LG | ≥ 992px | Desktop pequeño |
| XL | ≥ 1200px | Desktop |
| XXL | ≥ 1400px | Desktop grande |

### Clases Responsivas

```
/* Flexbox responsivo */
.d-flex.flex-column-sm { display: flex; flex-direction: column; }
@media (min-width: 576px) {
  .d-flex.flex-column-sm { flex-direction: row; }
}

/* Ocultar elementos en móvil */
.d-none-sm { display: none; }
@media (min-width: 576px) {
  .d-none-sm { display: block; }
}
```

---

## 📧 Validación de Formularios

### Validación en Cliente

```
@using System.ComponentModel.DataAnnotations

<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
	<DataAnnotationsValidator />
	<ValidationSummary />

	<div class="form-group">
		<label>Email</label>
		<InputText @bind-Value="model.Email" class="form-control" />
		<ValidationMessage For="@(() => model.Email)" />
	</div>

	<button type="submit" class="btn btn-primary">Enviar</button>
</EditForm>
```

### Atributos de Validación

```
public class CreateBudgetDTO
{
	[Required(ErrorMessage = "La categoría es requerida")]
	public Guid CategoryId { get; set; }

	[Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
	public decimal Amount { get; set; }

	[RegularExpression(@"^(Monthly|Yearly|Quarterly)$")]
	public string Period { get; set; }
}
```

---

## 🔄 Gestión de Estado

### Componentes con Estado

```
public partial class BudgetsPage : ComponentBase
{
	private List<BudgetDTO> budgets = new();
	private bool isLoading = false;
	private bool showModal = false;

	protected override async Task OnInitializedAsync()
	{
		await LoadBudgets();
	}

	private async Task LoadBudgets()
	{
		isLoading = true;
		try
		{
			budgets = await HttpClientService.GetAsync<List<BudgetDTO>>("/api/budgets");
		}
		finally
		{
			isLoading = false;
		}
	}
}
```

---

## 🚀 Inicio de la Aplicación

### Program.cs

```
builder.Services
	.AddScoped<AuthService>()
	.AddScoped<HttpClientService>()
	.AddScoped<ThemeService>()
	.AddScoped<ToastService>()
	.AddAuthorizationCore();
```

### App.razor

```
<Router AppAssembly="@typeof(App).Assembly">
	<Found Context="routeData">
		<RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
	</Found>
	<NotFound>
		<PageTitle>Página no encontrada</PageTitle>
		<p>Lo sentimos, la página solicitada no existe.</p>
	</NotFound>
</Router>
```

---

## 🛠️ Stack Tecnológico

| Componente | Tecnología | Versión |
|-----------|-----------|---------|
| **Framework** | Blazor WebAssembly | .NET 10 |
| **Lenguaje** | C# | 13.0 |
| **CSS Framework** | Bootstrap | 5.3 |
| **HTTP Client** | HttpClient | Nativo |
| **Validación** | DataAnnotations | Nativo |
| **Gráficos** | Chart.js (opcional) | - |
| **Almacenamiento Local** | LocalStorage API | Nativo |

---

## 📚 Estructura de Datos (DTOs)

### CategoryDTO
```
public class CategoryDTO
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}
```

### BudgetDTO
```
public class BudgetDTO
{
	public Guid Id { get; set; }
	public Guid CategoryId { get; set; }
	public decimal Amount { get; set; }
	public string Currency { get; set; }
	public string Period { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public DateTime CreatedAt { get; set; }
}
```

---

## 🔍 Características Avanzadas

### Búsqueda y Filtrado
- Filtrado por fecha (desde/hasta)
- Búsqueda por texto en descripciones
- Filtrado por categoría
- Rango de montos

### Paginación
- Seleccionar cantidad de registros por página (10, 25, 50)
- Navegación entre páginas
- Indicador de total de registros

### Ordenamiento
- Ordenar por cualquier columna
- Orden ascendente/descendente
- Indicadores visuales de orden actual

---

## 🎯 Mejores Prácticas

### Seguridad
- Validación en cliente y servidor
- Sanitización de inputs
- Protección contra XSS
- HTTPS obligatorio en producción

### Rendimiento
- Lazy loading de datos
- Caché de datos cuando sea aplicable
- Virtual scrolling para listas grandes
- Compresión de assets

### Accesibilidad
- Etiquetas ARIA apropiadas
- Contraste suficiente de colores
- Navegación por teclado completa
- Lectores de pantalla soportados

---

## 📞 Soporte y Contacto

- **GitHub**: [Linkgold/BudgetManager](https://github.com/Linkgold/BudgetManager)
- **Issues**: Reportar problemas en la pestaña Issues
- **Contribuciones**: Pull requests bienvenidos

---

## 📅 Última Actualización
Julio 2026

## 🔄 Versión UI
v1.0.0 - Blazor WebAssembly
