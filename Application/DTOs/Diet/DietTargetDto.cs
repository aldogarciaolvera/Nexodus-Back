namespace Nexodus_Back.Application.DTOs.Diet;

public class DietTargetDto
{
    public double TargetCalories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public double WaterTargetMl { get; set; }
    public string FastingProtocol { get; set; } = "None"; // "16:8", "18:6", "None"
    public string EatingWindowStart { get; set; } = "12:00"; // "12:00"
    public string EatingWindowEnd { get; set; } = "20:00";   // "20:00"
}
