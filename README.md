# Nexodus-Back

Backend API for Nexodus, a personal management application (Diet, Finance, TodoList, Workouts).

## Tecnologías y Herramientas

*   **.NET 10 (C#)**
*   **PostgreSQL**
*   **Entity Framework Core**
*   **JWT & BCrypt** para Autenticación
*   **FluentValidation** para la validación de peticiones
*   **Scalar** para la visualización de la API OpenAPI

## Arquitectura

El proyecto sigue estrictamente los principios de **Clean Architecture**:
*   `Core/`: Entidades de dominio, Excepciones e Interfaces.
*   `Application/`: DTOs, Casos de Uso (Servicios) y Validadores.
*   `Infrastructure/`: Repositorios, Contexto de Base de Datos y Proveedores de Seguridad.
*   `API/`: Endpoints (Minimal APIs), Middleware de Manejo de Excepciones e Inyección de Dependencias.

## Características Implementadas

*   **Identificadores UUID:** Todas las entidades (Usuarios, Finanzas, Dietas, etc.) utilizan `Guid` como identificadores únicos (`uuid` en Postgres).
*   **Autenticación y Seguridad:** 
    *   Registro y Login con validación.
    *   Generación de JWT y Refresh Tokens.
    *   Hasheo de contraseñas utilizando BCrypt.
*   **Manejo Global de Excepciones:** Middleware interceptor (`GlobalExceptionHandler`) para convertir excepciones personalizadas (`ValidationException`, `NotFoundException`, `UnauthorizedException`, etc.) en respuestas HTTP estandarizadas `ProblemDetails`.
*   **Validación:** Integración transparente de `FluentValidation` antes de ejecutar la lógica de los controladores.
*   **Módulo de Finanzas (CRUD Completo):** Endpoints para crear, leer, actualizar, eliminar y obtener resumen mensual de ingresos y gastos.

## Configuración y Ejecución

1.  **Clonar y configurar el entorno:**
    Copia el archivo de ejemplo de variables de entorno:
    ```bash
    cp .env.example .env
    ```
    Configura tus credenciales de base de datos PostgreSQL y JWT dentro del `.env`.

2.  **Migraciones de Base de Datos:**
    El proyecto utiliza Entity Framework Core. Para aplicar la base de datos:
    ```bash
    dotnet ef database update
    ```

3.  **Ejecutar:**
    ```bash
    dotnet run --launch-profile https
    ```
    Accede a la documentación OpenAPI (Scalar) navegando a la raíz o utilizando los endpoints correspondientes de tu entorno local.

## Docker

El proyecto incluye un `Dockerfile` optimizado y un `.dockerignore` para construir la imagen eficientemente utilizando un build multietapa (SDK 10.0 y ASP.NET 10.0 runtime).
