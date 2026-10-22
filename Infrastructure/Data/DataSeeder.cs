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
}
