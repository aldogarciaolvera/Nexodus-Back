using System.Text.Json;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedExercisesAsync(NexodusDbContext context, string contentRootPath)
    {
        if (context.Exercises.Any())
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
            Console.WriteLine("DataSeeder: exercises.json no encontrado.");
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
                GifS3Key = exElem.TryGetProperty("file", out var file) ? file.GetString() : null,
                ThumbS3Key = exElem.TryGetProperty("thumbUrl", out var thb) ? thb.GetString() : null
            };

            exercisesToInsert.Add(exercise);
        }

        await context.Exercises.AddRangeAsync(exercisesToInsert);
        await context.SaveChangesAsync();
        Console.WriteLine($"DataSeeder: {exercisesToInsert.Count} ejercicios insertados.");
    }
}
