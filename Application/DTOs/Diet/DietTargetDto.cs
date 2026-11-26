using System;

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
    public bool IsCalorieCyclingEnabled { get; set; } = true;
    public double WorkoutDayCarbMultiplier { get; set; } = 1.20;
    public double RestDayCarbMultiplier { get; set; } = 0.80;
    public decimal? FoodBudgetAmount { get; set; }
    public string FoodBudgetPeriod { get; set; } = "Weekly";
    public Guid? FoodBudgetCategoryId { get; set; }
    public string FoodBudgetCurrency { get; set; } = "MXN";
}

public class FoodBudgetSummaryDto
{
    public decimal? BudgetAmount { get; set; }
    public string Period { get; set; } = "Weekly";
    public string Currency { get; set; } = "MXN";
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public double PercentageUsed { get; set; }
    public decimal? SuggestedPerMealBudget { get; set; }
    public Guid? LinkedCategoryId { get; set; }
    public string? LinkedCategoryName { get; set; }
    public string PeriodStartDate { get; set; } = string.Empty;
    public string PeriodEndDate { get; set; } = string.Empty;
}
