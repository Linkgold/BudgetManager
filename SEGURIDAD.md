# Arquitectura de Seguridad

## Objetivo

Implementar una autenticación basada en JWT segura, sencilla de mantener y compatible con clientes de terceros (Blazor WebAssembly, aplicaciones móviles, escritorio, etc.).

---

# Autenticación

## Access Token (JWT)

Características:

- Formato: JWT
- Duración: 10-15 minutos
- Enviado mediante:

```
Authorization: Bearer {token}
```

No se renueva en cada petición.

---

## Renovación del Token

En cada petición autenticada:

1. Validar el JWT.
2. Comprobar el tiempo restante para su expiración.
3. Si el token ha consumido al menos el **80 % de su tiempo de vida**, generar un nuevo JWT.
4. Devolverlo únicamente mediante la cabecera:

```
X-New-Access-Token: {nuevoToken}
```

Si todavía no ha alcanzado el umbral de renovación, responder normalmente sin generar un nuevo token.

### Ejemplo

| Vida del JWT | Renovar cuando hayan transcurrido |
|--------------|-----------------------------------|
| 10 minutos | 8 minutos |
| 15 minutos | 12 minutos |
| 30 minutos | 24 minutos |

Este enfoque hace que la política de renovación sea independiente de la duración configurada para el JWT y evita modificar el código si dicha duración cambia en el futuro.

---

## Tokens concurrentes

No invalidar el JWT anterior al generar uno nuevo.

Motivos:

- Evita problemas con múltiples peticiones simultáneas.
- El token anterior expirará naturalmente pocos minutos después.

---

# Almacenamiento del Token

## Cliente Blazor WebAssembly

Opciones:

### Opción recomendada

Guardar el JWT únicamente en memoria.

Ventajas:

- No permanece almacenado en el navegador.
- Reduce el riesgo de robo mediante XSS.

Inconveniente:

- Al recargar la página será necesario volver a iniciar sesión.

---

### Opción alternativa

Guardar el JWT en LocalStorage.

Solo recomendable si:

- La aplicación necesita mantener la sesión tras un F5.
- Se implementan correctamente todas las medidas contra XSS descritas en este documento.

---

# Refresh Tokens

No se utilizarán.

Motivos:

- La API será consumida por terceros.
- Se busca mantener una autenticación sencilla.
- La renovación automática del Access Token cubre el escenario de sesión activa.

---

# Protección frente a XSS

La principal amenaza para una SPA es el Cross Site Scripting (XSS).

La mejor protección consiste en impedir la ejecución de código JavaScript inyectado.

---

## Content Security Policy (CSP)

Aplicar una política CSP estricta.

Ejemplo:

```html
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
">
```

En producción sustituir:

```
https://localhost:7131
```

por

```
https://api.midominio.com
```

---

## Evitar

Nunca utilizar:

```
unsafe-eval
```

Evitar también, si es posible:

```
unsafe-inline
```

en `style-src`, salvo que alguna librería lo requiera.

Nunca utilizar:

```
unsafe-inline
```

en `script-src`.

---

# Buenas prácticas Blazor

Evitar utilizar:

```
MarkupString
```

con contenido procedente de:

- usuarios
- bases de datos
- APIs externas

Si es imprescindible mostrar HTML, sanitizar previamente el contenido.

---

# HTTPS

Obligatorio.

Nunca permitir tráfico HTTP.

---

# Cabeceras recomendadas

Además de CSP:

```
Referrer-Policy: strict-origin-when-cross-origin
```

```
X-Content-Type-Options: nosniff
```

```
Permissions-Policy
```

Deshabilitar permisos no utilizados:

- cámara
- micrófono
- geolocalización
- etc.

---

# Gestión del Token

Flujo:

Login
   │
   ▼
JWT
   │
   ▼
Cliente
   │
Authorization: Bearer
   │
   ▼
API
   │
¿Ha consumido el 80 % de su vida útil?
│
├── No → Responder normalmente
│
└── Sí
      │
      ├── Generar nuevo JWT
      └── Enviar:
          X-New-Access-Token
                │
                ▼
      El cliente sustituye el token anterior

---

# Objetivos de esta arquitectura

✔ Compatible con clientes de terceros.

✔ Compatible con Blazor WebAssembly.

✔ Sin Refresh Tokens.

✔ Renovación transparente.

✔ Menor número de JWT generados.

✔ Protección frente a XSS mediante CSP.

✔ Arquitectura sencilla.

✔ Fácil de documentar para terceros.

---

# Mejoras futuras

Posibles mejoras si el proyecto crece:

- Revocación de sesiones.
- Lista negra temporal de JWT.
- Detección de reutilización de tokens.
- Auditoría de autenticaciones.
- Rate Limiting.
- Detección de IP sospechosas.
- Integración con IdentityServer / OpenIddict si la autenticación evoluciona.