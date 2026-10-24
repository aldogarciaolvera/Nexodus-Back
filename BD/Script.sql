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
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT "UQ_Categories_UserId_Name" UNIQUE (UserId, Name)
);

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
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "UQ_DietTargets_UserId" UNIQUE ("UserId")
);

CREATE TABLE "Foods" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" TEXT NOT NULL,
    "Brand" TEXT,
    "Barcode" TEXT,
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
