# Arquitectura del Proyecto (.NET)

Este proyecto promueve una Clean Architecture.

## Estructura Sugerida

- `Core/`: Entidades de dominio, interfaces de repositorios y servicios, y reglas de negocio. Excepciones personalizadas (`Core/Exceptions/`). (Sin dependencias externas).
- `Application/`: Casos de uso (Servicios), DTOs (Data Transfer Objects), interfaces de aplicación y validadores (`FluentValidation`).
- `Infrastructure/`: Implementación de repositorios, acceso a datos (Entity Framework Core, PostgreSQL), y servicios externos/seguridad (JWT, Hasheo).
- `API/`: Minimal APIs, configuración de inyección de dependencias (`Program.cs`), y middleware global de excepciones (`GlobalExceptionHandler`).

## Módulos del Dominio Actuales

1. **Auth & User**: Autenticación JWT, registro, login y gestión de perfil.
2. **Finances**: Gestión de ingresos y gastos, y resumen mensual.
3. **Categories**: Categorías personalizadas por usuario para finanzas.
4. **TodoList & Habits**: Tareas puntuales y hábitos recurrentes con rachas.
5. **Notes**: Diario personal e ideas (con checklist).
6. **Workouts**: Registro de entrenamientos (duración y calorías).
7. **Diet**: Registro de comidas y calorías.

## Reglas y Convenciones

- **Excepciones Controladas**: Nunca regresar "Internal Server Error" para reglas de negocio. Lanzar excepciones custom desde `Core/Exceptions` (e.g. `NotFoundException`) que el `GlobalExceptionHandler` convierte en `ProblemDetails`.
- **Validaciones**: Se resuelven mediante `FluentValidation` en el endpoint antes de pasar a la capa de Aplicación.
- **Inyección de Dependencias**: Todos los servicios y repositorios se inyectan como *Scoped* en `Program.cs`.
