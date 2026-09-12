---
name: global-rules
description: Reglas críticas del sistema, restricciones y directrices de comportamiento que el agente debe seguir SIEMPRE en cualquier interacción de este proyecto.
---

# Nexodus Backend - AI Agent Instructions

## 1. Context & Clean Architecture
* Use .NET 10 and C#.
* Strictly follow Clean Architecture principles to ensure separation of concerns.
* `Core/`: Entities, domain interfaces, and business rules. Must have zero external dependencies.
* `Application/`: Use cases, DTOs, mapping, and application interfaces.
* `Infrastructure/`: Data access (e.g., Entity Framework Core), external APIs, and platform-specific implementations.
* `API/`: Controllers/Minimal APIs, middleware, and dependency injection setup (`Program.cs`).
* Always follow the architecture laid out in `ARCHITECTURE.md`. Do not change anything from it. If any referenced folders do not exist, create them immediately before writing logic.

## 2. API Design & Data Flow
* Expose standard RESTful endpoints or Minimal APIs.
* Always return consistent, strongly-typed JSON responses.
* strictly adhere to standard HTTP status codes (200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 404 Not Found, 500 Internal Server Error).
* Implement a global exception handling middleware to catch errors and prevent exposing stack traces to the client.

## 3. Coding Standards
* Keep the `API/` layer thin: controllers and endpoints must only handle routing and HTTP concerns. Delegate all business logic to the `Application/` layer.
* Inject all services, repositories, and configurations using the native .NET Dependency Injection (DI) container.
* Always use asynchronous programming (`async`/`await`) for I/O operations (database queries, external API calls, file access).
* Utilize modern C# features (e.g., file-scoped namespaces, global usings, primary constructors, collection expressions).
* Never bypass the `Application/` layer to access `Infrastructure/` directly from `API/`.

## 4. README.md
* Always read the `README.md` and add the latest changes or update the information if it has changed.

## 5. Skills
* Always utilize the installed skills whenever possible.

## 6. .NET 10 Ecosystem
* Read and adhere to the latest .NET 10 official documentation for performance optimizations and modern ASP.NET Core practices before implementing complex architectural components.