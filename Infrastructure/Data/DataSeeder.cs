using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Infrastructure.Data;

/// <summary>
/// Seeds initial static data into the database asynchronously.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedExercisesAsync(NexodusDbContext context, string contentRootPath, ILogger? logger = null)
    {
        if (await context.Exercises.AnyAsync())
        {
            return; // Ya hay datos
        }

        var filePath = Path.Combine(contentRootPath, "BD", "Esquemas", "exercises.json");
        
        // Si no está en Esquemas, intentamos en la ruta anterior BD/exercises.json
        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(contentRootPath, "BD", "exercises.json");
        }

        if (!File.Exists(filePath))
        {
            logger?.LogWarning("DataSeeder: exercises.json no encontrado en {Path}", filePath);
            return;
        }

        var jsonData = await File.ReadAllTextAsync(filePath);
        using var jsonDoc = JsonDocument.Parse(jsonData);
        
        var exercisesArray = jsonDoc.RootElement.GetProperty("exercises").EnumerateArray();
        var exercisesToInsert = new List<Exercise>();

        foreach (var exElem in exercisesArray)
        {
            var externalId = exElem.GetProperty("id").GetString() ?? "";
            
            // Extraer las instrucciones y unirlas si es un arreglo
            string? instructionsStr = null;
            if (exElem.TryGetProperty("instructions", out var instructionsElem) && instructionsElem.ValueKind == JsonValueKind.Array)
            {
                var instructionsList = new List<string>();
                foreach (var inst in instructionsElem.EnumerateArray())
                {
                    instructionsList.Add(inst.GetString() ?? "");
                }
                instructionsStr = string.Join("\n", instructionsList);
            }

            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                ExternalId = externalId,
                Name = exElem.GetProperty("name").GetString() ?? "",
                BodyPart = exElem.TryGetProperty("bodyPart", out var bp) ? bp.GetString() : null,
                Muscle = exElem.TryGetProperty("muscle", out var mu) ? mu.GetString() : null,
                Equipment = exElem.TryGetProperty("equipment", out var eq) ? eq.GetString() : null,
                Category = exElem.TryGetProperty("category", out var cat) ? cat.GetString() : null,
                Instructions = instructionsStr,
                GifS3Key = $"{externalId}.gif",
                ThumbS3Key = $"{externalId}.thumb.webp"
            };

            exercisesToInsert.Add(exercise);
        }

        await context.Exercises.AddRangeAsync(exercisesToInsert);
        await context.SaveChangesAsync();
        logger?.LogInformation("DataSeeder: {Count} ejercicios insertados exitosamente.", exercisesToInsert.Count);
    }

    public static async Task SeedFoodCategoriesAsync(NexodusDbContext context, ILogger? logger = null)
    {
        // 1. Asegurar que las categorías de comida existan
        var existingCategories = await context.Categories
            .Where(c => c.Type == "Food")
            .ToListAsync();

        var systemCategories = new (string Name, string Description)[]
        {
            ("Carnes", "Carne de res, pollo, pavo, cerdo y carnes rojas o blancas"),
            ("Pescados y Mariscos", "Pescados blancos, salmón, atún, camarones y mariscos"),
            ("Verduras", "Verduras, hortalizas y vegetales frescos o cocidos"),
            ("Frutas", "Frutas frescas, bayas, plátanos, manzanas y cítricos"),
            ("Lácteos y Huevos", "Huevos, leche, quesos, yogur griego y requesón"),
            ("Cereales y Granos", "Arroz, avena, pasta, pan integral, quinoa y cereales"),
            ("Legumbres", "Frijoles, lentejas, garbanzos, soya y habas"),
            ("Frutos Secos y Semillas", "Nueces, almendras, cacahuates, chía y linaza"),
            ("Grasas y Aceites", "Aceite de oliva, aguacate, mantequilla y aceites saludables"),
            ("Bebidas", "Agua, café, té, infusiones y bebidas sin azúcar"),
            ("Suplementos", "Proteína en polvo (whey), creatina, multivitaminas y aminoácidos"),
            ("Snacks y Dulces", "Barras de proteína, botanas, galletas y chocolates"),
            ("Otros", "Condimentos, especias, aderezos y alimentos varios")
        };

        var now = DateTime.UtcNow;
        var toInsert = new List<Category>();

        foreach (var (name, desc) in systemCategories)
        {
            var found = existingCategories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                var newCat = new Category
                {
                    Id = Guid.NewGuid(),
                    UserId = null,
                    Name = name,
                    Description = desc,
                    Type = "Food",
                    CreatedAt = now,
                    UpdatedAt = now
                };
                toInsert.Add(newCat);
                existingCategories.Add(newCat);
            }
        }

        if (toInsert.Count > 0)
        {
            await context.Categories.AddRangeAsync(toInsert);
            await context.SaveChangesAsync();
            logger?.LogInformation("DataSeeder: {Count} categorías de comida insertadas exitosamente.", toInsert.Count);
        }

        // 2. Asociar categorías a los alimentos existentes que tengan Category null
        var foodsWithoutCategory = await context.Foods
            .Where(f => f.CategoryId == null && string.IsNullOrEmpty(f.Category))
            .ToListAsync();

        if (foodsWithoutCategory.Count > 0)
        {
            var catMap = existingCategories.ToDictionary(c => c.Name.ToLower(), c => c);

            foreach (var food in foodsWithoutCategory)
            {
                var n = food.Name.ToLower();
                string targetCat = "Otros";

                if (n.Contains("pollo") || n.Contains("res") || n.Contains("carne") || n.Contains("cerdo")) targetCat = "Carnes";
                else if (n.Contains("atún") || n.Contains("salmón") || n.Contains("pescado") || n.Contains("marisco")) targetCat = "Pescados y Mariscos";
                else if (n.Contains("huevo") || n.Contains("yogur") || n.Contains("leche") || n.Contains("queso")) targetCat = "Lácteos y Huevos";
                else if (n.Contains("arroz") || n.Contains("avena") || n.Contains("pan") || n.Contains("pasta") || n.Contains("grano")) targetCat = "Cereales y Granos";
                else if (n.Contains("manzana") || n.Contains("plátano") || n.Contains("fruta") || n.Contains("fresa")) targetCat = "Frutas";
                else if (n.Contains("aguacate") || n.Contains("aceite")) targetCat = "Grasas y Aceites";
                else if (n.Contains("proteína") || n.Contains("whey") || n.Contains("creatina")) targetCat = "Suplementos";
                else if (n.Contains("clorets") || n.Contains("dulce") || n.Contains("snack")) targetCat = "Snacks y Dulces";

                if (catMap.TryGetValue(targetCat.ToLower(), out var catObj))
                {
                    food.CategoryId = catObj.Id;
                    food.Category = catObj.Name;
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("DataSeeder: {Count} alimentos existentes actualizados con su categoría.", foodsWithoutCategory.Count);
        }
    }

    public static async Task SeedFoodsAsync(NexodusDbContext context, ILogger? logger = null)
    {
        if (await context.Foods.AnyAsync())
        {
            return; // Ya hay alimentos en la base de datos
        }

        var defaultFoods = new List<Food>
        {
            new() { Id = Guid.NewGuid(), Name = "Pechuga de Pollo Cocida", Brand = "Genérico", Barcode = "7501000111222", ServingSize = 100, ServingUnit = "g", Calories = 165, Protein = 31, Carbs = 0, Fat = 3.6, SodiumMg = 74, PotassiumMg = 256, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Huevo Entero Cocido", Brand = "Genérico", Barcode = "7501000111223", ServingSize = 100, ServingUnit = "g", Calories = 155, Protein = 13, Carbs = 1.1, Fat = 11, SodiumMg = 124, PotassiumMg = 126, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Avena en Hojuelas", Brand = "Quaker", Barcode = "7501000111224", ServingSize = 100, ServingUnit = "g", Calories = 389, Protein = 16.9, Carbs = 66.3, Fat = 6.9, Fiber = 10.6, SodiumMg = 2, PotassiumMg = 429, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Arroz Blanco Cocido", Brand = "Genérico", Barcode = "7501000111225", ServingSize = 100, ServingUnit = "g", Calories = 130, Protein = 2.7, Carbs = 28.2, Fat = 0.3, Fiber = 0.4, SodiumMg = 1, PotassiumMg = 35, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Atún en Agua Drenado", Brand = "Dolores", Barcode = "7501000111226", ServingSize = 100, ServingUnit = "g", Calories = 116, Protein = 25.5, Carbs = 0, Fat = 1, SodiumMg = 334, PotassiumMg = 237, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Plátano Maduro", Brand = "Genérico", Barcode = "7501000111227", ServingSize = 100, ServingUnit = "g", Calories = 89, Protein = 1.1, Carbs = 22.8, Fat = 0.3, Fiber = 2.6, SodiumMg = 1, PotassiumMg = 358, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Manzana Roja", Brand = "Genérico", Barcode = "7501000111228", ServingSize = 100, ServingUnit = "g", Calories = 52, Protein = 0.3, Carbs = 13.8, Fat = 0.2, Fiber = 2.4, SodiumMg = 1, PotassiumMg = 107, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Proteína Whey Vainilla", Brand = "Optimum Nutrition", Barcode = "748927028669", ServingSize = 30, ServingUnit = "g", Calories = 120, Protein = 24, Carbs = 3, Fat = 1.5, SodiumMg = 130, PotassiumMg = 160, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Carne de Res Molida Magra (90/10)", Brand = "Genérico", Barcode = "7501000111229", ServingSize = 100, ServingUnit = "g", Calories = 176, Protein = 26, Carbs = 0, Fat = 10, SodiumMg = 66, PotassiumMg = 350, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Salmón Fresco a la Plancha", Brand = "Genérico", Barcode = "7501000111230", ServingSize = 100, ServingUnit = "g", Calories = 206, Protein = 22, Carbs = 0, Fat = 12, SodiumMg = 59, PotassiumMg = 384, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Yogur Griego Natural Sin Azúcar", Brand = "Chobani", Barcode = "7501000111231", ServingSize = 100, ServingUnit = "g", Calories = 59, Protein = 10, Carbs = 3.6, Fat = 0.4, SodiumMg = 36, PotassiumMg = 141, IsCustom = false },
            new() { Id = Guid.NewGuid(), Name = "Aguacate Hass", Brand = "Genérico", Barcode = "7501000111232", ServingSize = 100, ServingUnit = "g", Calories = 160, Protein = 2, Carbs = 8.5, Fat = 14.7, Fiber = 6.7, SodiumMg = 7, PotassiumMg = 485, IsCustom = false }
        };

        await context.Foods.AddRangeAsync(defaultFoods);
        await context.SaveChangesAsync();
        logger?.LogInformation("DataSeeder: {Count} alimentos base insertados exitosamente.", defaultFoods.Count);
    }
}
