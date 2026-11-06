namespace Nexodus_Back.Application.DTOs.Diet;

public class CalculateTargetRequestDto
{
    public double Weight { get; set; } // in kg, e.g. 75
    public double Height { get; set; } // in cm, e.g. 175
    public int Age { get; set; } // e.g. 26
    public string BiologicalSex { get; set; } = "Male"; // "Male", "Female"
    public string ActivityLevel { get; set; } = "Moderate"; // "Sedentary", "Light", "Moderate", "Active", "VeryActive"
    public string Goal { get; set; } = "Maintenance"; // "Cut", "Maintenance", "Bulk"
}

public class CalculatedTargetResponseDto
{
    public double TargetCalories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public double WaterTargetMl { get; set; }
    public double Bmr { get; set; }
    public double Tdee { get; set; }
    public string FastingProtocol { get; set; } = "16:8";
    public string EatingWindowStart { get; set; } = "12:00";
    public string EatingWindowEnd { get; set; } = "20:00";
}
