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

### Endpoints Disponibles

**Autenticación (`/api/auth`)**
*   `POST /api/auth/register` - Registra un nuevo usuario.
    ```json
    {
      "username": "string",
      "email": "user@example.com",
      "password": "password123",
      "phoneNumber": "string" // (Opcional)
    }
    ```
*   `POST /api/auth/login` - Inicia sesión y devuelve un token JWT y un Refresh Token.
    ```json
    {
      "email": "user@example.com",
      "password": "password123"
    }
    ```
*   `POST /api/auth/refresh` - Renueva el token JWT utilizando un Refresh Token.
    ```json
    {
      "token": "string (JWT expirado)",
      "refreshToken": "string"
    }
    ```

**Sistema**
*   `GET /health` - Endpoint para comprobar el estado del servicio (Health Check).

**Finanzas (`/api/finances`)** *(Requieren Autenticación JWT)*
*   `GET /api/finances/` - Obtiene todas las finanzas del usuario autenticado.
*   `GET /api/finances/summary` - Obtiene el resumen mensual (Total de Ingresos, Gastos y Balance).
*   `GET /api/finances/{id}` - Obtiene los detalles de una transacción financiera específica.
*   `POST /api/finances/` - Crea una nueva transacción financiera (Ingreso/Gasto).
    ```json
    {
      "transactionType": "Income o Expense",
      "amount": 0.0,
      "category": "string", // (Opcional)
      "transactionDate": "2024-03-15T12:00:00Z" // (Opcional)
    }
    ```
*   `PUT /api/finances/{id}` - Actualiza una transacción financiera existente.
    ```json
    {
      "transactionType": "Income o Expense",
      "amount": 0.0,
      "category": "string", // (Opcional)
      "transactionDate": "2024-03-15T12:00:00Z" // (Opcional)
    }
    ```
*   `DELETE /api/finances/{id}` - Elimina una transacción financiera.

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
