CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "Email" VARCHAR(100) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "PhoneNumber" VARCHAR(20) NULL,
    "Role" VARCHAR(50) NOT NULL DEFAULT 'User',
    "Weight" DOUBLE PRECISION DEFAULT 0,
    "Height" DOUBLE PRECISION DEFAULT 0,
    "Dieta" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE "RefreshTokens" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Token" VARCHAR(255) NOT NULL,
    "JwtId" VARCHAR(255) NOT NULL,
    "CreationDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ExpiryDate" TIMESTAMPTZ NOT NULL,
    "Used" BOOLEAN NOT NULL DEFAULT FALSE,
    "Invalidated" BOOLEAN NOT NULL DEFAULT FALSE,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE
);

CREATE TABLE TodoList (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id"),
    Task TEXT NOT NULL,
    Subtitle VARCHAR(255),
    Tag VARCHAR(50),
    Urgent BOOLEAN DEFAULT FALSE,
    NotificationsEnabled BOOLEAN DEFAULT FALSE,
    NotificationTime VARCHAR(5),
    IsCompleted BOOLEAN DEFAULT FALSE,
    DueDate TIMESTAMPTZ,
    IsHabit BOOLEAN DEFAULT FALSE,
    Frequency VARCHAR(50),
    CustomDays VARCHAR(255),
    CurrentStreak INT DEFAULT 0,
    HighestStreak INT DEFAULT 0,
    LastCompletedAt TIMESTAMPTZ,
    PreviousCompletedAt TIMESTAMPTZ,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE Workouts (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id"),
    WorkoutType TEXT NOT NULL,
    Duration INT, -- duración en minutos
    CaloriesBurned INT,
    WorkoutDate TIMESTAMPTZ DEFAULT NOW(),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE Diet (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id"),
    MealType TEXT NOT NULL, -- desayuno, almuerzo, cena, etc.
    Calories INT,
    MealDate TIMESTAMPTZ DEFAULT NOW(),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE Categories (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id") ON DELETE CASCADE,
    Name TEXT NOT NULL,
    Description TEXT,
    MonthlyLimit NUMERIC(10, 2),
    Type VARCHAR(20) NOT NULL DEFAULT 'Money',
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX "IX_Categories_UserId_Name" ON Categories(UserId, Name);
CREATE INDEX "IX_Categories_Type" ON Categories(Type);

CREATE TABLE Finances (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id") ON DELETE CASCADE,
    TransactionType TEXT NOT NULL, -- ingreso o gasto
    Amount NUMERIC(10, 2) NOT NULL,
    Description TEXT,
    CategoryId UUID REFERENCES Categories(Id) ON DELETE SET NULL,
    PaymentMethod TEXT NOT NULL DEFAULT '',
    TransactionDate TIMESTAMPTZ DEFAULT NOW(),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE Notes (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id"),
    Type VARCHAR(50) NOT NULL, -- 'idea' or 'diario'
    Title VARCHAR(255),
    Content TEXT,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE ChecklistItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    NoteId UUID REFERENCES Notes(Id) ON DELETE CASCADE,
    Text TEXT NOT NULL,
    IsCompleted BOOLEAN DEFAULT FALSE
);

CREATE TABLE Exercises (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ExternalId VARCHAR(255) UNIQUE NOT NULL,
    Name VARCHAR(255) NOT NULL,
    BodyPart VARCHAR(100),
    Muscle VARCHAR(100),
    Equipment VARCHAR(100),
    Category VARCHAR(100),
    Instructions TEXT,
    GifS3Key VARCHAR(500),
    ThumbS3Key VARCHAR(500)
);

CREATE TABLE Routines (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id") ON DELETE CASCADE,
    Name VARCHAR(255) NOT NULL,
    Description TEXT,
    DifficultyLevel VARCHAR(50),
    TargetDay INTEGER NULL,
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE RoutineExercises (
    RoutineId UUID REFERENCES Routines(Id) ON DELETE CASCADE,
    ExerciseId UUID REFERENCES Exercises(Id) ON DELETE CASCADE,
    Sets INT NOT NULL DEFAULT 1,
    Reps INT NOT NULL DEFAULT 1,
    RestTimeInSeconds INT NOT NULL DEFAULT 60,
    Weight DOUBLE PRECISION NOT NULL DEFAULT 0,
    PRIMARY KEY (RoutineId, ExerciseId)
);

CREATE TABLE "WorkoutLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID REFERENCES "Users"("Id") ON DELETE CASCADE,
    "RoutineId" UUID REFERENCES "Routines"("Id") ON DELETE CASCADE,
    "DurationInSeconds" INT NOT NULL,
    "CompletedExercisesCount" INT NOT NULL,
    "DateCompleted" TIMESTAMPTZ DEFAULT NOW(),
    "CreatedAt" TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE "DietTargets" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "TargetCalories" DOUBLE PRECISION NOT NULL,
    "ProteinGrams" DOUBLE PRECISION NOT NULL,
    "CarbsGrams" DOUBLE PRECISION NOT NULL,
    "FatGrams" DOUBLE PRECISION NOT NULL,
    "WaterTargetMl" DOUBLE PRECISION NOT NULL,
    "FastingProtocol" TEXT NOT NULL DEFAULT 'None',
    "EatingWindowStart" TEXT NOT NULL DEFAULT '12:00',
    "EatingWindowEnd" TEXT NOT NULL DEFAULT '20:00',
    "IsCalorieCyclingEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "RestDayCarbMultiplier" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "WorkoutDayCarbMultiplier" DOUBLE PRECISION NOT NULL DEFAULT 0.0,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_DietTargets_UserId" UNIQUE ("UserId")
);

CREATE TABLE "Foods" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" TEXT NOT NULL,
    "Brand" TEXT,
    "Barcode" TEXT,
    "Category" TEXT,
    "CategoryId" UUID REFERENCES Categories(Id) ON DELETE SET NULL,
    "ServingSize" DOUBLE PRECISION NOT NULL DEFAULT 100,
    "ServingUnit" TEXT NOT NULL DEFAULT 'g',
    "Calories" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Protein" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Carbs" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Fat" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Fiber" DOUBLE PRECISION,
    "SodiumMg" DOUBLE PRECISION,
    "PotassiumMg" DOUBLE PRECISION,
    "IsCustom" BOOLEAN NOT NULL DEFAULT FALSE,
    "UserId" UUID REFERENCES "Users"("Id") ON DELETE SET NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX "IX_Foods_Barcode" ON "Foods"("Barcode");
CREATE INDEX "IX_Foods_Name" ON "Foods"("Name");
CREATE INDEX "IX_Foods_CategoryId" ON "Foods"("CategoryId");

CREATE TABLE "Meals" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Date" TEXT NOT NULL,
    "MealType" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Time" TEXT NOT NULL,
    "IsCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "Calories" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Protein" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Carbs" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Fat" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX "IX_Meals_UserId_Date" ON "Meals"("UserId", "Date");

CREATE TABLE "MealItems" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MealId" UUID NOT NULL REFERENCES "Meals"("Id") ON DELETE CASCADE,
    "FoodId" UUID REFERENCES "Foods"("Id") ON DELETE SET NULL,
    "FoodName" TEXT NOT NULL,
    "Quantity" DOUBLE PRECISION NOT NULL,
    "Unit" TEXT NOT NULL DEFAULT 'g',
    "Calories" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Protein" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Carbs" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Fat" DOUBLE PRECISION NOT NULL DEFAULT 0
);
CREATE INDEX "IX_MealItems_MealId" ON "MealItems"("MealId");

CREATE TABLE "WaterLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Date" TEXT NOT NULL,
    "ConsumedMl" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_WaterLogs_UserId_Date" UNIQUE ("UserId", "Date")
);

CREATE TABLE "FastingLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "StartTime" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "EndTime" TIMESTAMPTZ,
    "TargetHours" DOUBLE PRECISION NOT NULL DEFAULT 16,
    "FastingProtocol" TEXT NOT NULL DEFAULT '16:8',
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX "IX_FastingLogs_UserId_IsActive" ON "FastingLogs"("UserId", "IsActive");

CREATE TABLE "WorkoutSessions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "RoutineId" UUID REFERENCES "Routines"("Id") ON DELETE SET NULL,
    "Name" TEXT NOT NULL DEFAULT 'Entrenamiento',
    "StartTime" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "EndTime" TIMESTAMPTZ,
    "DurationSeconds" INT NOT NULL DEFAULT 0,
    "TotalVolumeKg" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "TotalReps" INT NOT NULL DEFAULT 0,
    "CompletedSetsCount" INT NOT NULL DEFAULT 0,
    "XpEarned" INT NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "Notes" TEXT
);
CREATE INDEX "IX_WorkoutSessions_RoutineId" ON "WorkoutSessions"("RoutineId");
CREATE INDEX "IX_WorkoutSessions_StartTime" ON "WorkoutSessions"("StartTime");
CREATE INDEX "IX_WorkoutSessions_UserId_IsActive" ON "WorkoutSessions"("UserId", "IsActive");

CREATE TABLE "WorkoutSets" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WorkoutSessionId" UUID NOT NULL REFERENCES "WorkoutSessions"("Id") ON DELETE CASCADE,
    "ExerciseId" UUID NOT NULL REFERENCES "Exercises"("Id") ON DELETE RESTRICT,
    "SetOrder" INT NOT NULL DEFAULT 1,
    "Type" INT NOT NULL DEFAULT 0, -- 0: Normal, 1: Warmup, 2: DropSet, 3: Failure, 4: Timed
    "WeightKg" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Reps" INT NOT NULL DEFAULT 0,
    "Rpe" DOUBLE PRECISION,
    "DurationSeconds" INT,
    "CalculatedOneRepMax" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "IsCompleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CompletedAt" TIMESTAMPTZ
);
CREATE INDEX "IX_WorkoutSets_ExerciseId" ON "WorkoutSets"("ExerciseId");
CREATE INDEX "IX_WorkoutSets_ExerciseId_IsCompleted" ON "WorkoutSets"("ExerciseId", "IsCompleted");
CREATE INDEX "IX_WorkoutSets_WorkoutSessionId" ON "WorkoutSets"("WorkoutSessionId");

CREATE TABLE "UserGamifications" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "CurrentStreak" INT NOT NULL DEFAULT 0,
    "HighestStreak" INT NOT NULL DEFAULT 0,
    "LastActiveDate" TIMESTAMPTZ,
    "ScheduledRestDays" TEXT NOT NULL DEFAULT 'Saturday,Sunday',
    "StreakGracePeriodExpiresAt" TIMESTAMPTZ,
    "StreakFreezesAvailable" INT NOT NULL DEFAULT 2,
    "NxpTotal" INT NOT NULL DEFAULT 0,
    "Tier" TEXT NOT NULL DEFAULT 'Initiate',
    "Cyphers" INT NOT NULL DEFAULT 100,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_UserGamifications_UserId" UNIQUE ("UserId")
);

CREATE TABLE "Recipes" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID REFERENCES "Users"("Id") ON DELETE CASCADE,
    "ParentRecipeId" UUID REFERENCES "Recipes"("Id") ON DELETE SET NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "MealType" TEXT NOT NULL,
    "DietType" TEXT NOT NULL,
    "PrepTimeMinutes" INT NOT NULL DEFAULT 0,
    "CookTimeMinutes" INT NOT NULL DEFAULT 0,
    "Servings" INT NOT NULL DEFAULT 1,
    "TotalCalories" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "TotalProtein" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "TotalCarbs" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "TotalFat" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Difficulty" TEXT NOT NULL DEFAULT 'Easy',
    "IsCommunity" BOOLEAN NOT NULL DEFAULT FALSE,
    "Tags" TEXT NOT NULL DEFAULT '[]',
    "ImageUrl" TEXT,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX "IX_Recipes_UserId" ON "Recipes"("UserId");
CREATE INDEX "IX_Recipes_MealType" ON "Recipes"("MealType");
CREATE INDEX "IX_Recipes_DietType" ON "Recipes"("DietType");
CREATE INDEX "IX_Recipes_IsCommunity" ON "Recipes"("IsCommunity");
CREATE INDEX "IX_Recipes_ParentRecipeId" ON "Recipes"("ParentRecipeId");

CREATE TABLE "RecipeIngredients" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RecipeId" UUID NOT NULL REFERENCES "Recipes"("Id") ON DELETE CASCADE,
    "FoodId" UUID REFERENCES "Foods"("Id") ON DELETE SET NULL,
    "CustomFoodName" TEXT NOT NULL,
    "CookingState" TEXT NOT NULL DEFAULT 'Raw',
    "Quantity" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Unit" TEXT NOT NULL DEFAULT 'g',
    "Calories" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Protein" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Carbs" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Fat" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "IsOptional" BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX "IX_RecipeIngredients_RecipeId" ON "RecipeIngredients"("RecipeId");
CREATE INDEX "IX_RecipeIngredients_FoodId" ON "RecipeIngredients"("FoodId");

CREATE TABLE "RecipeSteps" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RecipeId" UUID NOT NULL REFERENCES "Recipes"("Id") ON DELETE CASCADE,
    "StepNumber" INT NOT NULL,
    "Instruction" TEXT NOT NULL
);
CREATE INDEX "IX_RecipeSteps_RecipeId_StepNumber" ON "RecipeSteps"("RecipeId", "StepNumber");

CREATE TABLE "PantryItems" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "FoodId" UUID REFERENCES "Foods"("Id") ON DELETE SET NULL,
    "Name" TEXT NOT NULL,
    "Quantity" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Unit" TEXT NOT NULL DEFAULT 'g',
    "Category" TEXT NOT NULL DEFAULT 'Other',
    "ExpirationDate" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX "IX_PantryItems_UserId" ON "PantryItems"("UserId");
CREATE INDEX "IX_PantryItems_FoodId" ON "PantryItems"("FoodId");

CREATE TABLE "Habits" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Name" TEXT NOT NULL,
    "Description" TEXT,
    "Icon" TEXT NOT NULL,
    "Color" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "HabitType" TEXT NOT NULL, -- 'Build' o 'Quit'
    "TrackingType" TEXT NOT NULL, -- 'Boolean', 'Numeric', 'Timer'
    "TargetValue" DOUBLE PRECISION NOT NULL DEFAULT 1,
    "TargetUnit" TEXT,
    "OnTapAction" TEXT NOT NULL DEFAULT 'ToggleOrAddOne',
    "FrequencyType" TEXT NOT NULL DEFAULT 'Daily', -- 'Daily', 'SpecificDays', 'TimesPerPeriod', 'RepeatInterval'
    "FrequencyDays" TEXT,
    "FrequencyTargetCount" INT NOT NULL DEFAULT 1,
    "EveryNDaysInterval" INT NOT NULL DEFAULT 1,
    "StreakUnit" TEXT NOT NULL DEFAULT 'Days',
    "StreakGoal" INT,
    "FillMetPeriods" BOOLEAN NOT NULL DEFAULT FALSE,
    "AutoCompleteSource" TEXT, -- ej. 'Nexodus.Water', 'Nexodus.Gym', 'Nexodus.Diet'
    "CurrentStreak" INT NOT NULL DEFAULT 0,
    "BestStreak" INT NOT NULL DEFAULT 0,
    "LastCompletedDate" TIMESTAMPTZ,
    "IsArchived" BOOLEAN NOT NULL DEFAULT FALSE,
    "OrderIndex" INT NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX "IX_Habits_UserId_IsArchived" ON "Habits"("UserId", "IsArchived");
CREATE INDEX "IX_Habits_AutoCompleteSource" ON "Habits"("AutoCompleteSource");

CREATE TABLE "HabitCompletions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "HabitId" UUID NOT NULL REFERENCES "Habits"("Id") ON DELETE CASCADE,
    "UserId" UUID NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
    "Date" TEXT NOT NULL, -- 'yyyy-MM-dd'
    "Amount" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "Status" TEXT NOT NULL DEFAULT 'Completed', -- 'Completed', 'Partial', 'Skipped', 'Failed'
    "Note" TEXT,
    "LoggedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_HabitCompletions_HabitId_Date" UNIQUE ("HabitId", "Date")
);
CREATE INDEX "IX_HabitCompletions_UserId_Date" ON "HabitCompletions"("UserId", "Date");

CREATE TABLE "HabitReminders" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "HabitId" UUID NOT NULL REFERENCES "Habits"("Id") ON DELETE CASCADE,
    "Time" TEXT NOT NULL, -- 'HH:mm'
    "DaysOfWeek" TEXT,
    "IsEnabled" BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE INDEX "IX_HabitReminders_HabitId" ON "HabitReminders"("HabitId");
