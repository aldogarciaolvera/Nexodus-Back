using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Infrastructure.Configuration;

namespace Nexodus_Back.Infrastructure.Services;

public class OpenFoodFactsService(
    IHttpClientFactory httpClientFactory,
    IOptions<OpenFoodFactsOptions> options,
    ILogger<OpenFoodFactsService> logger) : IOpenFoodFactsService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly OpenFoodFactsOptions _options = options?.Value ?? new OpenFoodFactsOptions();
    private readonly ILogger<OpenFoodFactsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Food?> LookupBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        var cleanBarcode = barcode.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(cleanBarcode, @"^[0-9A-Za-z-]{4,32}$"))
        {
            _logger.LogWarning("Código de barras con formato inválido o potencialmente malicioso descartado: '{Barcode}'", cleanBarcode);
            return null;
        }

        var client = _httpClientFactory.CreateClient("OpenFoodFacts");
        var requestUrl = $"{_options.BaseUrl.TrimEnd('/')}/{Uri.EscapeDataString(cleanBarcode)}.json";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("User-Agent", _options.UserAgent);

            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Open Food Facts returned HTTP {StatusCode} for barcode '{Barcode}'.", response.StatusCode, cleanBarcode);
                return null;
            }

            var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
            var root = doc.RootElement;

            // Open Food Facts returns status == 1 when product exists
            if (!root.TryGetProperty("status", out var statusProp) || statusProp.GetInt32() != 1)
            {
                return null;
            }

            if (!root.TryGetProperty("product", out var product))
            {
                return null;
            }

            // Extract Name
            string name = string.Empty;
            if (product.TryGetProperty("product_name_es", out var nameEs) && !string.IsNullOrWhiteSpace(nameEs.GetString()))
            {
                name = nameEs.GetString()!.Trim();
            }
            else if (product.TryGetProperty("product_name", out var nameDefault) && !string.IsNullOrWhiteSpace(nameDefault.GetString()))
            {
                name = nameDefault.GetString()!.Trim();
            }
            else if (product.TryGetProperty("product_name_en", out var nameEn) && !string.IsNullOrWhiteSpace(nameEn.GetString()))
            {
                name = nameEn.GetString()!.Trim();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Producto ({cleanBarcode})";
            }

            // Extract Brand
            string? brand = null;
            if (product.TryGetProperty("brands", out var brandProp) && !string.IsNullOrWhiteSpace(brandProp.GetString()))
            {
                brand = brandProp.GetString()!.Trim();
            }

            // Extract Serving Size
            double servingSize = 100.0;
            string servingUnit = "g";

            // Extract Nutrients per 100g
            double calories = 0;
            double protein = 0;
            double carbs = 0;
            double fat = 0;
            double? fiber = null;
            double? sodiumMg = null;
            double? potassiumMg = null;

            if (product.TryGetProperty("nutriments", out var nutriments))
            {
                calories = GetNutrientValue(nutriments, "energy-kcal_100g", "energy-kcal");
                if (calories <= 0)
                {
                    // Fallback to energy in kJ divided by 4.184
                    var energyKj = GetNutrientValue(nutriments, "energy_100g", "energy");
                    if (energyKj > 0)
                    {
                        calories = Math.Round(energyKj / 4.184, 1);
                    }
                }

                protein = GetNutrientValue(nutriments, "proteins_100g", "proteins");
                carbs = GetNutrientValue(nutriments, "carbohydrates_100g", "carbohydrates");
                fat = GetNutrientValue(nutriments, "fat_100g", "fat");

                var fiberVal = GetNutrientValue(nutriments, "fiber_100g", "fiber");
                if (fiberVal > 0) fiber = fiberVal;

                var sodiumVal = GetNutrientValue(nutriments, "sodium_100g", "sodium");
                if (sodiumVal > 0) sodiumMg = Math.Round(sodiumVal * 1000, 1); // convert g to mg

                var potassiumVal = GetNutrientValue(nutriments, "potassium_100g", "potassium");
                if (potassiumVal > 0) potassiumMg = Math.Round(potassiumVal * 1000, 1);
            }

            return new Food
            {
                Id = Guid.NewGuid(),
                Name = name,
                Brand = brand,
                Barcode = cleanBarcode,
                ServingSize = servingSize,
                ServingUnit = servingUnit,
                Calories = Math.Round(calories, 1),
                Protein = Math.Round(protein, 1),
                Carbs = Math.Round(carbs, 1),
                Fat = Math.Round(fat, 1),
                Fiber = fiber.HasValue ? Math.Round(fiber.Value, 1) : null,
                SodiumMg = sodiumMg,
                PotassiumMg = potassiumMg,
                IsCustom = false,
                UserId = null,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching barcode '{Barcode}' from Open Food Facts API.", cleanBarcode);
            return null;
        }
    }

    private static double GetNutrientValue(JsonElement nutriments, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (nutriments.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDouble(out var num))
                    return num;
                if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out var parsed))
                    return parsed;
            }
        }
        return 0.0;
    }
}

