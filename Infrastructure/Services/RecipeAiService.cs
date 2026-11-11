using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexodus_Back.Application.DTOs.Diet;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Configuration;

namespace Nexodus_Back.Infrastructure.Services;

/// <summary>
/// AI service powered by Google Gemini for smart recipe generation matching remaining daily macros and pantry inventory.
/// </summary>
public class RecipeAiService(
    IHttpClientFactory httpClientFactory,
    IOptions<GeminiOptions> options,
    IPantryRepository pantryRepo,
    ILogger<RecipeAiService> logger) : IRecipeAiService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly GeminiOptions _options = options?.Value ?? new GeminiOptions();
    private readonly IPantryRepository _pantryRepo = pantryRepo ?? throw new ArgumentNullException(nameof(pantryRepo));
    private readonly ILogger<RecipeAiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<RecipeDto> GenerateRecipeForRemainingMacrosAsync(
        Guid userId,
        MatchMacrosRecipeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        EnsureApiKeyConfigured();

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Eres Nexodus Chef AI, un nutricionista deportivo de élite y chef profesional.");
        promptBuilder.AppendLine("Tu objetivo es diseñar una receta deliciosa, realista y fácil de cocinar para un atleta.");
        promptBuilder.AppendLine("La receta DEBE ajustarse lo más cerca posible a los siguientes macronutrientes restantes del día:");
        promptBuilder.AppendLine($"- Calorías objetivo: ~{Math.Max(100, request.RemainingCalories):0.#} kcal");
        promptBuilder.AppendLine($"- Proteína objetivo: ~{Math.Max(10, request.RemainingProtein):0.#} g");
        promptBuilder.AppendLine($"- Carbohidratos objetivo: ~{Math.Max(0, request.RemainingCarbs):0.#} g");
        promptBuilder.AppendLine($"- Grasas objetivo: ~{Math.Max(0, request.RemainingFat):0.#} g");
        promptBuilder.AppendLine($"- Tipo de comida: {request.MealType}");
        promptBuilder.AppendLine($"- Enfoque dietético: {request.DietType}");
        promptBuilder.AppendLine($"- Tiempo máximo de preparación y cocción: {Math.Max(10, request.MaxPrepTimeMinutes)} minutos.");

        if (!string.IsNullOrWhiteSpace(request.Preferences))
        {
            promptBuilder.AppendLine($"- Preferencias / Exclusiones del usuario: \"{request.Preferences.Trim()}\".");
        }

        promptBuilder.AppendLine("Instrucciones de cálculo:");
        promptBuilder.AppendLine("1. Desglosa los ingredientes en gramos exactos con su estado de cocción ('Raw', 'Boiled', 'Grilled', 'Fried', 'Baked', 'Steamed').");
        promptBuilder.AppendLine("2. Calcula las calorías y macronutrientes sumando los ingredientes reales.");
        promptBuilder.AppendLine("3. El resultado DEBE ser un JSON válido estructurado exactamente con el siguiente esquema:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"name\": \"Nombre del platillo en español\",");
        promptBuilder.AppendLine("  \"description\": \"Descripción apetitosa y explicación de por qué encaja en los macros\",");
        promptBuilder.AppendLine("  \"mealType\": \"Breakfast / Lunch / Dinner / Snack\",");
        promptBuilder.AppendLine("  \"dietType\": \"Standard / HighProtein / Keto / Vegan / LowCarb\",");
        promptBuilder.AppendLine("  \"prepTimeMinutes\": 10,");
        promptBuilder.AppendLine("  \"cookTimeMinutes\": 15,");
        promptBuilder.AppendLine("  \"servings\": 1,");
        promptBuilder.AppendLine("  \"difficulty\": \"Easy\",");
        promptBuilder.AppendLine("  \"tags\": [\"PostWorkout\", \"Rápido\", \"AltoEnProteína\"],");
        promptBuilder.AppendLine("  \"ingredients\": [");
        promptBuilder.AppendLine("    { \"customFoodName\": \"Pechuga de pollo\", \"cookingState\": \"Grilled\", \"quantity\": 160, \"unit\": \"g\", \"calories\": 264, \"protein\": 49.6, \"carbs\": 0, \"fat\": 5.8, \"isOptional\": false }");
        promptBuilder.AppendLine("  ],");
        promptBuilder.AppendLine("  \"steps\": [");
        promptBuilder.AppendLine("    { \"stepNumber\": 1, \"instruction\": \"Paso detallado de preparación\" }");
        promptBuilder.AppendLine("  ]");
        promptBuilder.AppendLine("}");

        var jsonResponse = await CallGeminiTextApiAsync(promptBuilder.ToString(), cancellationToken);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawResult = JsonSerializer.Deserialize<CreateRecipeDto>(jsonResponse, options)
            ?? throw new ValidationException("No se pudo interpretar la receta generada por el chef de IA.");

        var recipeId = Guid.NewGuid();
        var totalCalories = rawResult.Ingredients.Sum(i => i.Calories);
        var totalProtein = rawResult.Ingredients.Sum(i => i.Protein);
        var totalCarbs = rawResult.Ingredients.Sum(i => i.Carbs);
        var totalFat = rawResult.Ingredients.Sum(i => i.Fat);

        return new RecipeDto
        {
            Id = recipeId,
            UserId = userId,
            Name = rawResult.Name,
            Description = rawResult.Description,
            MealType = string.IsNullOrWhiteSpace(rawResult.MealType) ? request.MealType : rawResult.MealType,
            DietType = string.IsNullOrWhiteSpace(rawResult.DietType) ? request.DietType : rawResult.DietType,
            PrepTimeMinutes = rawResult.PrepTimeMinutes,
            CookTimeMinutes = rawResult.CookTimeMinutes,
            Servings = Math.Max(1, rawResult.Servings),
            TotalCalories = Math.Round(totalCalories, 1),
            TotalProtein = Math.Round(totalProtein, 1),
            TotalCarbs = Math.Round(totalCarbs, 1),
            TotalFat = Math.Round(totalFat, 1),
            Difficulty = rawResult.Difficulty,
            IsCommunity = false,
            Tags = rawResult.Tags ?? [],
            ImageUrl = "https://images.unsplash.com/photo-1498837167922-ddd27525d352?auto=format&fit=crop&w=800&q=80",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Ingredients = rawResult.Ingredients.Select(i => new RecipeIngredientDto
            {
                Id = Guid.NewGuid(),
                RecipeId = recipeId,
                CustomFoodName = i.CustomFoodName,
                CookingState = string.IsNullOrWhiteSpace(i.CookingState) ? "Raw" : i.CookingState,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Calories = i.Calories,
                Protein = i.Protein,
                Carbs = i.Carbs,
                Fat = i.Fat,
                IsOptional = i.IsOptional
            }).ToList(),
            Steps = rawResult.Steps.OrderBy(s => s.StepNumber).Select(s => new RecipeStepDto
            {
                Id = Guid.NewGuid(),
                RecipeId = recipeId,
                StepNumber = s.StepNumber,
                Instruction = s.Instruction
            }).ToList()
        };
    }

    public async Task<List<PantryCookSuggestionDto>> SuggestRecipesFromPantryAsync(
        Guid userId,
        PantryCookRequestDto request,
        CancellationToken cancellationToken = default)
    {
        EnsureApiKeyConfigured();

        var availableIngredients = new List<string>();

        if (request.UseUserPantry)
        {
            var userPantry = await _pantryRepo.GetByUserIdAsync(userId, cancellationToken);
            availableIngredients.AddRange(userPantry.Select(p => $"{p.Name} ({p.Quantity} {p.Unit})"));
        }

        if (request.Ingredients != null && request.Ingredients.Count > 0)
        {
            availableIngredients.AddRange(request.Ingredients.Where(i => !string.IsNullOrWhiteSpace(i)));
        }

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Eres Nexodus Chef en Modo Despensa ('Cocina con lo que tienes').");
        promptBuilder.AppendLine("Tu tarea es crear 3 opciones de recetas deliciosas y saludables utilizando prioritariamente los ingredientes que el usuario ya tiene a la mano.");
        promptBuilder.AppendLine("Ingredientes disponibles en la despensa del usuario:");
        if (availableIngredients.Count > 0)
        {
            promptBuilder.AppendLine(string.Join(", ", availableIngredients.Distinct()));
        }
        else
        {
            promptBuilder.AppendLine("(Despensa vacía)");
        }

        if (!string.IsNullOrWhiteSpace(request.FreeTextIngredients))
        {
            promptBuilder.AppendLine($"Texto directo del usuario: \"{request.FreeTextIngredients.Trim()}\".");
        }

        promptBuilder.AppendLine($"- Tipo de comida deseada: {request.MealType}");
        promptBuilder.AppendLine($"- Enfoque dietético: {request.DietType}");
        promptBuilder.AppendLine($"- Tiempo máximo: {request.MaxPrepTimeMinutes} minutos");
        promptBuilder.AppendLine("Reglas:");
        promptBuilder.AppendLine("1. Si a una receta le faltan ingredientes menores indispensables (como condimentos, pizca de sal, aceite o un complemento), márcalos explícitamente en 'missingIngredients' para que se añadan a su lista de compras.");
        promptBuilder.AppendLine("2. En 'usedIngredients', coloca los ingredientes de su despensa que sí se aprovecharon.");
        promptBuilder.AppendLine("3. En 'matchPercentage', pon un número de 0 a 100 indicando qué tan completa está la receta con su despensa.");
        promptBuilder.AppendLine("4. Devuelve ÚNICAMENTE un JSON con un array con 3 recetas bajo la clave 'recipes':");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"recipes\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"name\": \"Nombre del platillo\",");
        promptBuilder.AppendLine("      \"description\": \"Por qué es una gran opción con su despensa\",");
        promptBuilder.AppendLine("      \"mealType\": \"Lunch\",");
        promptBuilder.AppendLine("      \"dietType\": \"Standard\",");
        promptBuilder.AppendLine("      \"prepTimeMinutes\": 10,");
        promptBuilder.AppendLine("      \"cookTimeMinutes\": 15,");
        promptBuilder.AppendLine("      \"servings\": 1,");
        promptBuilder.AppendLine("      \"totalCalories\": 450,");
        promptBuilder.AppendLine("      \"totalProtein\": 38.0,");
        promptBuilder.AppendLine("      \"totalCarbs\": 40.0,");
        promptBuilder.AppendLine("      \"totalFat\": 12.0,");
        promptBuilder.AppendLine("      \"difficulty\": \"Easy\",");
        promptBuilder.AppendLine("      \"matchPercentage\": 85,");
        promptBuilder.AppendLine("      \"usedIngredients\": [\"Pollo\", \"Arroz\"],");
        promptBuilder.AppendLine("      \"missingIngredients\": [\"Aceite de oliva\", \"Ajo\"],");
        promptBuilder.AppendLine("      \"ingredients\": [");
        promptBuilder.AppendLine("        { \"customFoodName\": \"Pechuga de pollo\", \"cookingState\": \"Grilled\", \"quantity\": 150, \"unit\": \"g\", \"calories\": 247, \"protein\": 46.5, \"carbs\": 0, \"fat\": 5.4, \"isOptional\": false }");
        promptBuilder.AppendLine("      ],");
        promptBuilder.AppendLine("      \"steps\": [");
        promptBuilder.AppendLine("        { \"stepNumber\": 1, \"instruction\": \"Instrucción de cocina clara\" }");
        promptBuilder.AppendLine("      ]");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ]");
        promptBuilder.AppendLine("}");

        var jsonResponse = await CallGeminiTextApiAsync(promptBuilder.ToString(), cancellationToken);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        using var doc = JsonDocument.Parse(jsonResponse);
        if (doc.RootElement.TryGetProperty("recipes", out var recipesElem))
        {
            var list = JsonSerializer.Deserialize<List<PantryCookSuggestionDto>>(recipesElem.GetRawText(), jsonOptions);
            return list ?? [];
        }

        var directList = JsonSerializer.Deserialize<List<PantryCookSuggestionDto>>(jsonResponse, jsonOptions);
        return directList ?? [];
    }

    #region Private Gemini Invocation

    private void EnsureApiKeyConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ValidationException(
                "La clave GEMINI_API_KEY no está configurada en las variables de entorno del servidor. " +
                "Por favor, añade 'GEMINI_API_KEY' a tu archivo .env para utilizar el Chef IA de Nexodus."
            );
        }
    }

    private async Task<string> CallGeminiTextApiAsync(string prompt, CancellationToken cancellationToken)
    {
        var primaryModel = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-3.8-flash" : _options.Model.Trim();
        var candidateModels = new List<string> { primaryModel };
        foreach (var m in new[] { "gemini-3.8-flash", "gemini-3.5-flash-lite", "gemini-3.5-flash", "gemini-2.5-flash", "gemini-2.0-flash" })
        {
            if (!candidateModels.Contains(m))
            {
                candidateModels.Add(m);
            }
        }

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json",
                temperature = 0.3
            }
        };

        var client = _httpClientFactory.CreateClient("GeminiVision");
        var jsonPayload = JsonSerializer.Serialize(requestBody);

        HttpResponseMessage? lastResponse = null;
        string? lastResponseBody = null;
        string? successfulResponseBody = null;

        foreach (var candidateModel in candidateModels)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{candidateModel}:generateContent?key={_options.ApiKey}";
            try
            {
                using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, httpContent, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    successfulResponseBody = responseBody;
                    _logger.LogInformation("Gemini generó la receta exitosamente con el modelo '{Model}'.", candidateModel);
                    break;
                }

                lastResponse = response;
                lastResponseBody = responseBody;

                _logger.LogWarning("Gemini API devolvió HTTP {Status} con el modelo '{Model}'. Probando modelo de respaldo...", response.StatusCode, candidateModel);

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == (HttpStatusCode)429 ||
                    response.StatusCode == HttpStatusCode.NotFound ||
                    (int)response.StatusCode >= 500)
                {
                    await Task.Delay(350, cancellationToken);
                    continue;
                }

                // Errores de cliente fatales (400, 401, etc.)
                throw new ValidationException($"Error al comunicarse con Google Gemini API ({response.StatusCode}).");
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Fallo de conexión al invocar Gemini con el modelo '{Model}'.", candidateModel);
            }
        }

        if (string.IsNullOrWhiteSpace(successfulResponseBody))
        {
            var status = lastResponse?.StatusCode.ToString() ?? "Unknown";
            throw new ValidationException($"No se pudo generar la receta con Gemini ({status}). Intenta nuevamente.");
        }

        using var doc = JsonDocument.Parse(successfulResponseBody);
        var root = doc.RootElement;
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            throw new ValidationException("Google Gemini no generó respuesta para la solicitud.");
        }

        var textContent = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        if (string.IsNullOrWhiteSpace(textContent))
        {
            throw new ValidationException("Respuesta vacía recibida desde el Chef IA.");
        }

        var cleanJson = textContent.Trim();
        if (cleanJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleanJson = cleanJson[7..];
        }
        else if (cleanJson.StartsWith("```"))
        {
            cleanJson = cleanJson[3..];
        }

        if (cleanJson.EndsWith("```"))
        {
            cleanJson = cleanJson[..^3];
        }

        return cleanJson.Trim();
    }

    #endregion
}

