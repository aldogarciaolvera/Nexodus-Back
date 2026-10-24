using System.Text;
using Amazon.S3;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    options.UseNpgsql(connectionString));

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
builder.Services.AddScoped<IDietTargetRepository, DietTargetRepository>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IWaterLogRepository, WaterLogRepository>();
builder.Services.AddScoped<IFastingRepository, FastingRepository>();
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
builder.Services.AddScoped<IDietService, DietService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
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

app.UseExceptionHandler();

app.UseCors("AllowFrontend");

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
app.MapDietEndpoints();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NexodusDbContext>();
    await DataSeeder.SeedExercisesAsync(context, app.Environment.ContentRootPath, app.Logger);
    await DataSeeder.SeedFoodsAsync(context, app.Logger);
}

app.MapGet("/health", () => TypedResults.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithSummary("Comprueba el estado operativo del servicio")
    .WithTags("System");

app.Run();
