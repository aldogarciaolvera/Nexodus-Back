using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Amazon.S3;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nexodus_Back.API.Endpoints;
using Nexodus_Back.API.Middleware;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Application.Services;
using Nexodus_Back.Application.Validators;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Configuration;
using Nexodus_Back.Infrastructure.Data;
using Nexodus_Back.Infrastructure.Repositories;
using Nexodus_Back.Infrastructure.Security;
using Nexodus_Back.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Configure Database
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};Database={Environment.GetEnvironmentVariable("DB_NAME")};Username={Environment.GetEnvironmentVariable("DB_USER")};Password={Environment.GetEnvironmentVariable("DB_PASS")};Port={Environment.GetEnvironmentVariable("DB_PORT")};";
builder.Services.AddDbContext<NexodusDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Strongly-typed options
builder.Services.Configure<JwtOptions>(options =>
{
    options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "";
    options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "";
    options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "";
    options.ExpirationMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"), out var exp) ? exp : 60;
});

builder.Services.Configure<S3Options>(options =>
{
    options.AccessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? "";
    options.SecretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? "";
    options.ServiceUrl = Environment.GetEnvironmentVariable("S3_ENDPOINT") ?? "";
    options.BucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "nexodus";
});

builder.Services.Configure<GeminiOptions>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
    options.Model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-3.5-flash-lite";
});

builder.Services.Configure<OpenFoodFactsOptions>(options =>
{
    options.BaseUrl = Environment.GetEnvironmentVariable("OPEN_FOOD_FACTS_URL") ?? "https://world.openfoodfacts.org/api/v2/product";
    options.UserAgent = Environment.GetEnvironmentVariable("OPEN_FOOD_FACTS_USER_AGENT") ?? "Nexodus-Nutrition/1.0 (contact@nexodus.app)";
});

builder.Services.AddHttpClient();

// Configure JSON options to support String Enums in Minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure Authentication & Authorization
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret)) throw new InvalidOperationException("JWT_SECRET is not set.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// Exception Handling & Problem Details
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// S3 Client Singleton
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Options = sp.GetRequiredService<IOptions<S3Options>>().Value;
    var serviceUrl = s3Options.ServiceUrl;
    var config = new AmazonS3Config
    {
        ServiceURL = serviceUrl.StartsWith("http") ? serviceUrl : $"https://{serviceUrl}",
        ForcePathStyle = true
    };
    return new AmazonS3Client(s3Options.AccessKey, s3Options.SecretKey, config);
});

// Core & Infrastructure Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IFinanceRepository, FinanceRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITodoListRepository, TodoListRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IRoutineRepository, RoutineRepository>();
builder.Services.AddScoped<IWorkoutLogRepository, WorkoutLogRepository>();
builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
builder.Services.AddScoped<IDietTargetRepository, DietTargetRepository>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IWaterLogRepository, WaterLogRepository>();
builder.Services.AddScoped<IFastingRepository, FastingRepository>();
builder.Services.AddScoped<IUserGamificationRepository, UserGamificationRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IPantryRepository, PantryRepository>();
builder.Services.AddScoped<IStorageService, S3StorageService>();

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITodoListService, TodoListService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IRoutineService, RoutineService>();
builder.Services.AddScoped<IWorkoutLogService, WorkoutLogService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IWorkoutSessionService, WorkoutSessionService>();
builder.Services.AddScoped<IDietService, DietService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IPantryService, PantryService>();
builder.Services.AddScoped<IRecipeAiService, RecipeAiService>();
builder.Services.AddScoped<IOpenFoodFactsService, OpenFoodFactsService>();
builder.Services.AddScoped<IGeminiVisionService, GeminiVisionService>();

// Configure CORS with explicit allowed origins (SEC-01)
var configuredOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
var defaultOrigins = new[] { "http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:3000", "http://127.0.0.1:5173" };
var allowedOrigins = !string.IsNullOrWhiteSpace(configuredOrigins)
    ? configuredOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : defaultOrigins;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Rate Limiting (SEC-02)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Auth endpoints: 10 requests per minute by client IP
    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous_auth",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // AI endpoints: 10 requests per minute by user or IP
    options.AddPolicy("AiPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous_ai",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("", options =>
    {
        options.HideSearch = true;
        options.HideModels = true;
        options.HideDarkModeToggle = true;
    });
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Security Transport & Content Headers (SEC-07)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("X-XSS-Protection", "0");
    await next();
});

app.UseExceptionHandler();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapAuthEndpoints();
app.MapFinanceEndpoints();
app.MapCategoryEndpoints();
app.MapUserEndpoints();
app.MapTodoListEndpoints();
app.MapNoteEndpoints();
app.MapExerciseEndpoints();
app.MapRoutineEndpoints();
app.MapWorkoutLogEndpoints();
app.MapWorkoutSessionEndpoints();
app.MapDietEndpoints();
app.MapRecipeEndpoints();
app.MapPantryEndpoints();
app.MapGamificationEndpoints();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NexodusDbContext>();
    await DataSeeder.SeedExercisesAsync(context, app.Environment.ContentRootPath, app.Logger);
    await DataSeeder.SeedFoodCategoriesAsync(context, app.Logger);
    await DataSeeder.SeedFoodsAsync(context, app.Logger);
    await DataSeeder.SeedRecipesAsync(context, app.Logger);
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithSummary("Comprueba el estado operativo del servicio")
    .WithTags("System");

app.Run();
