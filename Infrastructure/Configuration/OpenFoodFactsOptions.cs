namespace Nexodus_Back.Infrastructure.Configuration;

public class OpenFoodFactsOptions
{
    public string BaseUrl { get; set; } = "https://world.openfoodfacts.org/api/v2/product";
    public string UserAgent { get; set; } = "Nexodus-Nutrition/1.0 (contact@nexodus.app)";
}

