using System;
using System.Collections.Generic;
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
using Nexodus_Back.Infrastructure.Configuration;

namespace Nexodus_Back.Infrastructure.Services;

public class GeminiVisionService(
    IHttpClientFactory httpClientFactory,
    IOptions<GeminiOptions> options,
    ILogger<GeminiVisionService> logger) : IGeminiVisionService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly GeminiOptions _options = options?.Value ?? new GeminiOptions();
    private readonly ILogger<GeminiVisionService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<FoodAnalysisResultDto> AnalyzeMealImageAsync(
        string base64Image,
        string? mimeType,
        string? additionalContext,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ValidationException(
                "La clave GEMINI_API_KEY no está configurada en las variables de entorno del servidor. " +
                "Por favor, añade 'GEMINI_API_KEY' a tu archivo .env para utilizar Nexodus Lens."
            );
        }

        if (string.IsNullOrWhiteSpace(base64Image))
        {
            throw new ValidationException("La imagen en base64 no puede estar vacía.");
        }

        // Clean any data URI scheme prefixes (e.g., 'data:image/jpeg;base64,')
        var cleanBase64 = base64Image.Trim();
        var commaIndex = cleanBase64.IndexOf(',');
        if (commaIndex >= 0 && cleanBase64[..commaIndex].Contains("base64", StringComparison.OrdinalIgnoreCase))
        {
            cleanBase64 = cleanBase64[(commaIndex + 1)..].Trim();
        }

        var determinedMimeType = string.IsNullOrWhiteSpace(mimeType) ? "image/jpeg" : mimeType.Trim();

        var primaryModel = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-3.5-flash-lite" : _options.Model.Trim();
        var candidateModels = new List<string> { primaryModel };
        if (!candidateModels.Contains("gemini-3.5-flash-lite"))
        {
            candidateModels.Add("gemini-3.5-flash-lite");
        }
        if (!candidateModels.Contains("gemini-3.5-flash"))
        {
            candidateModels.Add("gemini-3.5-flash");
        }

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Eres Nexodus Lens, el analizador de visión computacional y nutrición deportiva de precisión de la plataforma Nexodus.");
        promptBuilder.AppendLine("Analiza minuciosamente la imagen del plato de comida o bebida adjunto.");
        promptBuilder.AppendLine("Identifica el platillo general y desglosa cada ingrediente visible o deducible, estimando con realismo su gramaje, calorías y macronutrientes (proteína, carbohidratos, grasas).");
        if (!string.IsNullOrWhiteSpace(additionalContext))
        {
            promptBuilder.AppendLine($"Contexto o descripción aportada por el usuario: \"{additionalContext.Trim()}\".");
        }
        promptBuilder.AppendLine("Debes responder ÚNICAMENTE con un JSON válido estructurado exactamente con el siguiente esquema:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"dishName\": \"Nombre del platillo en español (ej. Pechuga de pollo a la plancha con arroz integral y aguacate)\",");
        promptBuilder.AppendLine("  \"totalCalories\": 540,");
        promptBuilder.AppendLine("  \"totalProtein\": 45.5,");
        promptBuilder.AppendLine("  \"totalCarbs\": 48.0,");
        promptBuilder.AppendLine("  \"totalFat\": 14.2,");
        promptBuilder.AppendLine("  \"confidence\": 0.92,");
        promptBuilder.AppendLine("  \"notes\": \"Breve observación nutricional o recomendación\",");
        promptBuilder.AppendLine("  \"items\": [");
        promptBuilder.AppendLine("    { \"foodName\": \"Pechuga de pollo\", \"estimatedGrams\": 180, \"calories\": 297, \"protein\": 38.0, \"carbs\": 0.0, \"fat\": 4.5 },");
        promptBuilder.AppendLine("    { \"foodName\": \"Arroz integral\", \"estimatedGrams\": 120, \"calories\": 140, \"protein\": 3.5, \"carbs\": 28.0, \"fat\": 1.2 }");
        promptBuilder.AppendLine("  ]");
        promptBuilder.AppendLine("}");

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = promptBuilder.ToString() },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = determinedMimeType,
                                data = cleanBase64
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json",
                temperature = 0.2
            }
        };

        var client = _httpClientFactory.CreateClient("GeminiVision");
        var jsonPayload = JsonSerializer.Serialize(requestBody);

        try
        {
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
                        _logger.LogInformation("Gemini Vision analizó exitosamente la imagen con el modelo '{Model}'.", candidateModel);
                        break;
                    }

                    lastResponse = response;
                    lastResponseBody = responseBody;

                    _logger.LogWarning(
                        "Gemini API devolvió HTTP {Status} con el modelo '{Model}'. Verificando siguiente modelo de respaldo...",
                        response.StatusCode, candidateModel
                    );

                    // Si es sobrecarga temporal (503 Service Unavailable, 429 Too Many Requests, 5xx) o 404 de modelo retirado
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == (HttpStatusCode)429 ||
                        response.StatusCode == HttpStatusCode.NotFound ||
                        (int)response.StatusCode >= 500)
                    {
                        await Task.Delay(350, cancellationToken);
                        continue;
                    }

                    // En otros errores de petición como 400 o 401, no continuar
                    break;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Excepción de red al consultar Gemini API con modelo '{Model}'. Probando modelo de respaldo...", candidateModel);
                    await Task.Delay(350, cancellationToken);
                }
            }

            if (successfulResponseBody == null)
            {
                _logger.LogError("Todos los modelos candidatos de Gemini Vision fallaron. Última respuesta ({Status}): {Body}",
                    lastResponse?.StatusCode, lastResponseBody);

                if (lastResponse?.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    throw new ValidationException(
                        "Los servidores de Google Gemini están experimentando alta demanda momentáneamente. " +
                        "Por favor vuelve a intentar en unos segundos."
                    );
                }

                throw new ValidationException($"Error al comunicarse con Google Gemini API ({lastResponse?.StatusCode.ToString() ?? "Error"}). Verifica tu clave API o intenta más tarde.");
            }

            using var doc = JsonDocument.Parse(successfulResponseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                throw new ValidationException("Google Gemini no generó ninguna respuesta para la imagen proporcionada.");
            }

            var firstCandidate = candidates[0];
            var parts = firstCandidate.GetProperty("content").GetProperty("parts");
            var textContent = parts[0].GetProperty("text").GetString();

            if (string.IsNullOrWhiteSpace(textContent))
            {
                throw new ValidationException("Respuesta vacía recibida desde el modelo de visión.");
            }

            // Clean markdown code blocks if present (```json ... ```)
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

            cleanJson = cleanJson.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<FoodAnalysisResultDto>(cleanJson, options);
            if (result == null)
            {
                throw new ValidationException("No se pudo interpretar el resultado nutricional de la imagen.");
            }

            return result;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error analyzing food image with Gemini.");
            throw new ValidationException("Ocurrió un error inesperado al analizar la imagen con Nexodus Lens.");
        }
    }
}

