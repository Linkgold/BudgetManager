# Deploy de BudgetManager en Azure

## Arquitectura

La solución está compuesta por dos aplicaciones independientes:

- **API** (ASP.NET Core Web API)
- **UI** (Blazor WebAssembly Standalone)

Cada una se publica en una Azure App Service distinta.

---

# 1. Publicar la API

## 1.1 Comprobar appsettings.Production.json

Verificar que existen los valores correctos para producción.

Ejemplo:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://<url-ui>.azurewebsites.net"
    ]
  }
}
```

La URL debe ser exactamente la URL pública de la UI.

No utilizar:

```
https://localhost
```

ni valores de ejemplo como:

```
https://midominio.com
```

---

## 1.2 Publicar

Publicar la API mediante el perfil de publicación de Azure.

Al finalizar:

- Reiniciar la App Service (opcional, pero recomendable).

---

## 1.3 Comprobar

Abrir:

```
https://<url-api>/swagger
```

Debe mostrarse Swagger correctamente.

---

# 2. Publicar la UI

## 2.1 Comprobar appsettings.Production.json

Verificar que la URL de la API es correcta.

Ejemplo:

```json
{
  "ApiConfiguration": {
    "ApiUrl": "https://<url-api>.azurewebsites.net"
  }
}
```

No dejar nunca valores de ejemplo.

Ejemplo incorrecto:

```
https://midominio.com
```

---

## 2.2 Content Security Policy (CSP)

Si existe una CSP en `wwwroot/index.html`, revisar especialmente:

```text
connect-src
```

Debe permitir la URL de la API.

Durante el desarrollo puede deshabilitarse temporalmente para facilitar el diagnóstico.

---

## 2.3 Publicar

Publicar la UI mediante el perfil de publicación.

No es necesario copiar archivos manualmente.

---

# 3. Comprobaciones

Abrir la aplicación.

Comprobar:

- La página carga correctamente.
- No existen errores JavaScript.
- Se puede iniciar sesión.
- Se puede registrar un usuario.
- La UI puede comunicarse con la API.

---

# 4. Si aparece un error CORS

Abrir F12 → Network.

Comprobar:

- URL a la que realmente está llamando la UI.
- Código HTTP.
- Mensaje de respuesta.

Si el error indica:

```
No 'Access-Control-Allow-Origin'
```

verificar:

- Que la URL de la UI coincide exactamente con la configurada en:

```json
Cors:AllowedOrigins
```

Recordar que CORS compara:

- protocolo
- dominio
- puerto

Deben coincidir exactamente.

---

# 5. Comprobaciones rápidas

## API

✓ Swagger responde.

✓ CORS configurado.

✓ URL correcta.

---

## UI

✓ ApiUrl correcta.

✓ CSP correcta.

✓ Publicación correcta.

---

# Problemas encontrados durante el desarrollo

## CSP demasiado restrictiva

La política impedía que Blazor WebAssembly arrancase correctamente.

Solución:

- Revisar la CSP.
- O deshabilitarla temporalmente mientras se diagnostica el problema.

---

## ApiUrl incorrecta

En `appsettings.Production.json` permanecía el valor:

```
https://midominio.com
```

La UI realizaba todas las llamadas a esa dirección.

---

## CORS

La API únicamente permitía el origen:

```
https://budgetmanager-ui-dev.azurewebsites.net
```

Sin embargo, Azure había asignado una URL distinta:

```
https://budgetmanager-ui-dev-xxxxxxxx.spaincentral-01.azurewebsites.net
```

Fue necesario actualizar `AllowedOrigins`.

---

# Lecciones aprendidas

- No dejar nunca URLs de ejemplo en producción.
- Verificar siempre `appsettings.Production.json`.
- Revisar F12 → Network antes de modificar código.
- Un error de CORS puede ocultar un problema distinto (URL incorrecta, error 500, etc.).
- Comprobar siempre la URL exacta asignada por Azure a cada App Service.