namespace Nexodus_Back.Application.DTOs.Diet;

public class WaterLogDto
{
    public string Date { get; set; } = string.Empty;
    public double ConsumedMl { get; set; }
    public double TargetMl { get; set; }
    public double Percentage { get; set; }
}

public class AddWaterDto
{
    public string Date { get; set; } = string.Empty;
    public double AmountMl { get; set; } // e.g. +250 or +500
}

public class SetWaterDto
{
    public string Date { get; set; } = string.Empty;
    public double TotalMl { get; set; }
}
