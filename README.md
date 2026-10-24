# Nexodus-Back

Backend API for Nexodus, a personal management application (Diet, Finance, TodoList, Workouts).

## Tecnologías y Herramientas

*   **.NET 10 (C# 12+)** con características modernas (Primary Constructors, File-Scoped Namespaces, Collection Expressions).
*   **PostgreSQL** como motor relacional.
*   **Entity Framework Core** con consultas optimizadas (`.AsNoTracking()` y carga diferida selectiva).
*   **JWT & BCrypt** para autenticación y seguridad con `JwtProvider` y `PasswordHasher`.
*   **AWSSDK.S3** con cliente Singleton administrado y soporte para MinIO / Cloudflare R2 / AWS S3.
*   **FluentValidation** acoplado mediante Endpoint Filters (`ValidationFilter<T>`).
*   **Scalar** y OpenAPI nativo de .NET 10 para exploración interactiva de la API.

## Arquitectura

El proyecto sigue estrictamente los principios de **Clean Architecture**:
*   `Core/`: Entidades de dominio, Excepciones del catálogo (`ValidationException`, `NotFoundException`, `ConflictException`, `UnauthorizedException`) e Interfaces de repositorio. Cero dependencias externas.
*   `Application/`: DTOs organizados por módulo, Casos de Uso (Servicios con interfaces e inyección de dependencias `I...Service`) y Validadores FluentValidation.
*   `Infrastructure/`: Repositorios EF Core, Contexto de Base de Datos (`NexodusDbContext`), Servicios de almacenamiento (`S3StorageService`), Opciones de configuración fuertemente tipadas (`JwtOptions`, `S3Options`) y Seguridad.
*   `API/`: Minimal APIs delgadas con metadatos OpenAPI (`.WithName()`, `.WithSummary()`, `.Produces<T>()`), Filtros de endpoint (`ValidationFilter<T>`), Extensiones de `ClaimsPrincipal` y Middleware de Excepciones Globales (`GlobalExceptionHandler`).

## Características Implementadas

*   **Identificadores UUID:** Todas las entidades (Usuarios, Finanzas, Categorías, Rutinas, Ejercicios, etc.) utilizan `Guid` como identificadores únicos (`uuid` en Postgres).
*   **Autenticación y Seguridad:** 
    *   Registro y Login con validación automática.
    *   Generación de JWT y Refresh Tokens criptográficamente seguros.
    *   Hasheo de contraseñas utilizando BCrypt.
    *   Opciones fuertemente tipadas validadas (`JwtOptions`).
*   **Manejo Global de Excepciones y ProblemDetails:** Middleware interceptor (`GlobalExceptionHandler`) que captura excepciones controladas de dominio y errores imprevistos, transformándolos en respuestas estandarizadas **RFC 7807 / RFC 9457 ProblemDetails** e integrando logging estructurado con `ILogger`.
*   **Validación Transparente:** Integración de `ValidationFilter<T>` como filtro de Minimal API para ejecutar validaciones antes de invocar la lógica de negocio.
*   **Módulo de Finanzas (CRUD Completo):** Endpoints para transacciones y balance mensual. Compatible con tipos bilingües (`Income`/`Expense` e `Ingreso`/`Gasto`; `Tarjeta`/`Card` y `Efectivo`/`Cash`).
*   **Módulo de Categorías (CRUD):** Categorías personalizadas por usuario para transacciones financieras.
*   **Módulo de Tareas y Hábitos:** Gestión de tareas puntuales y hábitos recurrentes con seguimiento de rachas (`CurrentStreak`, `HighestStreak`).
*   **Módulo de Notas y Checklist:** Diario personal y notas con soporte para listas de verificación dinámicas.
*   **Módulo de Ejercicios y Rutinas:** Catálogo paginado de más de 1,300 ejercicios con generación concurrente y optimizada de URLs pre-firmadas en S3, diseño de rutinas personalizadas y bitácora de entrenamientos.

---

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

**Usuario (`/api/user`)** *(Requiere Autenticación JWT)*
*   `GET /api/user/me` - Obtiene los datos del usuario autenticado (nombre, email, teléfono, peso, altura, dieta).
*   `PUT /api/user/me` - Actualiza los datos del usuario autenticado.
    ```json
    {
      "username": "newUsername",
      "email": "new@example.com",
      "phoneNumber": "0987654321", // (Opcional)
      "weight": 70.5, // (Opcional, en kg)
      "height": 1.75, // (Opcional, en metros)
      "dieta": true // (Opcional, booleano para indicar si cuenta con dieta configurada)
    }
    ```

**Sistema**
*   `GET /health` - Endpoint para comprobar el estado del servicio (Health Check).

**Finanzas (`/api/finances`)** *(Requieren Autenticación JWT)*
*   `GET /api/finances/` - Obtiene todas las finanzas del usuario autenticado.
*   `GET /api/finances/summary` - Obtiene el resumen mensual (Total de Ingresos, Gastos y Balance).
*   `GET /api/finances/{id}` - Obtiene los detalles de una transacción específica.
*   `POST /api/finances/` - Crea una nueva transacción (admite `Ingreso`/`Income` o `Gasto`/`Expense`).
    ```json
    {
      "transactionType": "Income", // o "Ingreso", "Expense", "Gasto"
      "amount": 1500.50,
      "description": "string", // (Opcional)
      "categoryId": "string (UUID)", // (Opcional)
      "paymentMethod": "Tarjeta", // o "Card", "Efectivo", "Cash"
      "transactionDate": "2026-03-15T12:00:00Z" // (Opcional)
    }
    ```
*   `PUT /api/finances/{id}` - Actualiza una transacción financiera existente.
*   `DELETE /api/finances/{id}` - Elimina una transacción financiera.

**Categorías (`/api/categories`)** *(Requieren Autenticación JWT)*
*   `GET /api/categories/` - Obtiene todas las categorías del usuario.
*   `GET /api/categories/{id}` - Obtiene los detalles de una categoría específica.
*   `POST /api/categories/` - Crea una nueva categoría.
    ```json
    {
      "name": "string",
      "description": "string", // (Opcional)
      "monthlyLimit": 0.0 // (Opcional)
    }
    ```
*   `PUT /api/categories/{id}` - Actualiza una categoría existente.
*   `DELETE /api/categories/{id}` - Elimina una categoría.

**Lista de Tareas y Hábitos (`/api/todos`)** *(Requieren Autenticación JWT)*
*   `GET /api/todos/` - Obtiene todas las tareas y hábitos del usuario.
*   `GET /api/todos/{id}` - Obtiene una tarea o hábito por ID.
*   `POST /api/todos/` - Crea una nueva tarea o hábito.
    ```json
    {
      "task": "Leer 10 páginas",
      "subtitle": "Capítulo 1", // (Opcional)
      "tag": "Lectura", // (Opcional)
      "urgent": false, // (Opcional)
      "notificationsEnabled": true, // (Opcional)
      "notificationTime": "08:00", // (Opcional, formato HH:mm)
      "dueDate": "2026-03-15T12:00:00Z", // (Opcional)
      "isHabit": true,
      "frequency": "Daily", // Daily, Weekly, Monthly, Custom
      "customDays": "Lunes,Miércoles" // (Opcional)
    }
    ```
*   `PUT /api/todos/{id}` - Actualiza una tarea o hábito.
*   `POST /api/todos/{id}/complete` - Marca una tarea o hábito como completado hoy (incrementa la racha).
*   `POST /api/todos/{id}/uncompleted` - Desmarca una tarea o hábito completado hoy (restaura la racha anterior).
*   `DELETE /api/todos/{id}` - Elimina la tarea o hábito.

**Notas y Diario (`/api/notes`)** *(Requieren Autenticación JWT)*
*   `GET /api/notes/` - Obtiene todas las notas del usuario.
*   `GET /api/notes/{id}` - Obtiene una nota específica por su ID.
*   `POST /api/notes/` - Crea una nueva nota o checklist.
    ```json
    {
      "type": "idea o diario",
      "title": "string", // (Opcional)
      "content": "string", // (Opcional)
      "checklist": [ // (Opcional)
        { "text": "Hacer la cama", "isCompleted": false }
      ]
    }
    ```
*   `PATCH /api/notes/{id}` - Actualiza una nota o sus items de checklist.
*   `DELETE /api/notes/{id}` - Elimina la nota.

**Ejercicios (`/api/exercises`)** *(Requieren Autenticación JWT)*
*   `GET /api/exercises?page=1&pageSize=50` - Obtiene el catálogo paginado de ejercicios con URLs pre-firmadas a S3.
*   `GET /api/exercises/{id}` - Obtiene un ejercicio específico por su ID.

**Rutinas (`/api/routines`)** *(Requieren Autenticación JWT)*
*   `GET /api/routines/` - Obtiene todas las rutinas del usuario.
*   `GET /api/routines/{id}` - Obtiene una rutina específica con todos sus ejercicios.
*   `POST /api/routines/` - Crea una nueva rutina y asocia sus ejercicios (con validación de series, reps y peso).
    ```json
    {
      "name": "string",
      "description": "string",
      "difficultyLevel": "string",
      "targetDay": 1, // (1 al 7)
      "exercises": [
        {
          "exerciseId": "string (UUID)",
          "sets": 4,
          "reps": 12,
          "restTimeInSeconds": 90,
          "weight": 25.5
        }
      ]
    }
    ```
*   `PUT /api/routines/{id}` - Actualiza la rutina y sus ejercicios.
*   `DELETE /api/routines/{id}` - Elimina la rutina.

**Historial de Entrenamientos (`/api/workouts`)** *(Requieren Autenticación JWT)*
*   `GET /api/workouts/` - Obtiene todos los registros de entrenamiento del usuario.
*   `GET /api/workouts/{id}` - Obtiene un registro de entrenamiento específico.
*   `POST /api/workouts/` - Registra un nuevo entrenamiento completado.
    ```json
    {
      "routineId": "string (UUID_DE_LA_RUTINA)",
      "durationInSeconds": 3600,
      "completedExercisesCount": 5,
      "dateCompleted": "2026-10-07T00:00:00Z"
    }
    ```
*   `DELETE /api/workouts/{id}` - Elimina un registro de entrenamiento.

**Nutrición y Dieta (`/api/diet`)** *(Requieren Autenticación JWT)*
*   **Metas y Biometría:**
    *   `GET /api/diet/target` - Obtiene las metas nutricionales y ventana de ingesta del usuario.
    *   `PUT /api/diet/target` - Actualiza manualmente calorías, macros (proteína, carbs, grasas), agua y protocolo de ayuno.
    *   `POST /api/diet/calculate-target` - Calcula TDEE y recomendación de macros según peso, altura, edad, sexo biológico, nivel de actividad y objetivo (`Cut`, `Maintenance`, `Bulk`).
*   **Comidas y Resumen Diario:**
    *   `GET /api/diet/daily?date=yyyy-MM-dd` - Obtiene resumen diario consolidado (totales de calorías, macros, agua, comidas agrupadas por tipo y metas activas).
    *   `POST /api/diet/meals` - Registra una comida con sus ingredientes (`Items`).
    *   `PUT /api/diet/meals/{id}` - Modifica alimentos, cantidades o marca la comida como completada.
    *   `DELETE /api/diet/meals/{id}` - Elimina una comida del registro diario.
*   **Catálogo de Alimentos:**
    *   `GET /api/diet/foods/search?query=pechuga&page=1&pageSize=20` - Búsqueda paginada de alimentos por nombre o marca.
    *   `GET /api/diet/foods/barcode/{barcode}` - Búsqueda instantánea por código de barras (EAN/UPC).
    *   `POST /api/diet/foods` - Crea un alimento personalizado con sus valores nutricionales por porción.
*   **Hidratación:**
    *   `GET /api/diet/water?date=yyyy-MM-dd` - Retorna los mililitros consumidos en la fecha y el porcentaje alcanzado.
    *   `POST /api/diet/water` - Suma una ingesta rápida de agua (ej. +250 ml o +500 ml).
    *   `PUT /api/diet/water/set` - Ajusta el total exacto consumido para la fecha.
*   **Ayuno Intermitente:**
    *   `GET /api/diet/fasting/status` - Estado en vivo del ayuno (horas transcurridas, restantes, % de progreso y hora estimada de finalización).
    *   `POST /api/diet/fasting/start` - Inicia una sesión de ayuno con protocolo (ej. `16:8`) y horas objetivo.
    *   `POST /api/diet/fasting/stop` - Concluye la sesión de ayuno activa y abre la ventana de ingesta.

---

## Configuración y Ejecución

1.  **Clonar y configurar el entorno:**
    Copia el archivo de ejemplo de variables de entorno:
    ```bash
    cp .env.example .env
    ```
    Configura tus credenciales de base de datos PostgreSQL, configuración JWT y **credenciales S3 (AWS SDK / MinIO)** dentro del `.env`.

2.  **Migraciones de Base de Datos y Seed:**
    Para aplicar las migraciones de Entity Framework Core:
    ```bash
    dotnet ef database update
    ```
    *Nota: Al correr la aplicación por primera vez, el `DataSeeder` inyectará automáticamente más de 1,300 ejercicios de forma asíncrona desde `BD/Esquemas/exercises.json` a la base de datos.*
    *Asimismo, en la carpeta `BD/` se encuentran `BD/Script.sql` (esquema DDL completo y sincronizado con PostgreSQL) y `BD/Curls.txt` (colección de comandos cURL listos para probar todos los endpoints).*

3.  **Ejecutar:**
    ```bash
    dotnet run --launch-profile https
    ```
    Accede a la documentación OpenAPI (Scalar) navegando a la raíz (`/`) de tu servidor local.

## Docker

El proyecto incluye un `Dockerfile` optimizado y un `.dockerignore` para construir la imagen eficientemente utilizando un build multietapa (SDK 10.0 y ASP.NET 10.0 runtime).
