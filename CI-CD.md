# CI/CD - BudgetManager

## Objetivo

Automatizar el proceso de compilación, pruebas y despliegue del proyecto mediante GitHub Actions y Azure App Service.

Actualmente la API dispone de un pipeline CI/CD completo.

---

# Flujo de trabajo

```
                    develop
                        │
                        │ Push
                        ▼
             GitHub Actions (CI)
             ├── Checkout
             ├── Build
             ├── Tests
             └── Resultado
                        │
                        ▼
             Pull Request → main
                        │
          Build y Tests obligatorios
                        │
                        ▼
                  Merge aprobado
                        │
                        ▼
                  Push a main
                        │
                        ▼
             GitHub Actions (CD)
             ├── Checkout
             ├── Build
             ├── Tests
             ├── Publish
             └── Deploy a Azure
```

---

# Ramas

## develop

Rama de desarrollo.

Todo el desarrollo diario se realiza sobre esta rama.

No despliega en Azure.

---

## main

Rama de producción.

No se permite realizar Push directamente.

Los cambios únicamente llegan mediante Pull Request desde `develop`.

Cada Push sobre esta rama provoca un despliegue automático.

---

# Pipeline de la API

Archivo:

```
.github/workflows/api-budgetmanager-dev.yml
```

## Eventos

### Pull Request hacia main

Se ejecuta:

- Build
- Tests

No se realiza despliegue.

Su objetivo es validar el código antes del Merge.

---

### Push sobre main

Se ejecuta:

- Build
- Tests
- Publish
- Deploy a Azure

Si los tests fallan, el despliegue no se realiza.

---

# Protección de la rama main

La rama `main` está protegida mediante un Ruleset.

Configuración principal:

- Restrict deletions
- Block force pushes
- Require a pull request before merging
- Require status checks to pass

El Pull Request únicamente puede fusionarse cuando el pipeline de CI finaliza correctamente.

---

# Azure

La API está desplegada en Azure App Service.

El despliegue se realiza mediante:

- GitHub Actions
- Azure Login (OIDC)
- Azure Web App Deploy

No se utilizan Publish Profiles.

---

# Autenticación con Azure

Se utiliza autenticación mediante OpenID Connect (OIDC).

Ventajas:

- No hay secretos de publicación.
- No hay credenciales almacenadas.
- Mayor seguridad.
- Recomendado por Microsoft.

---

# Flujo de desarrollo

1. Crear una rama desde `develop` (opcional).
2. Desarrollar la funcionalidad.
3. Hacer Push a `develop`.
4. Crear Pull Request hacia `main`.
5. GitHub ejecuta Build y Tests.
6. Si todo es correcto, realizar Merge.
7. GitHub despliega automáticamente la API en Azure.

---

# Mejoras futuras

- Pipeline independiente para la UI.
- Tests de integración.
- Entorno de Staging.
- Cobertura de código.
- Análisis estático.
- Versionado automático.
- Generación automática de Releases.