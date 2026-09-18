CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Username" VARCHAR(50) UNIQUE NOT NULL,
    "Email" VARCHAR(100) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "PhoneNumber" VARCHAR(20) NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE TodoList (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id"),
    Task TEXT NOT NULL,
    Subtitle VARCHAR(255),
    Tag VARCHAR(50),
    Urgent BOOLEAN DEFAULT FALSE,
    IsCompleted BOOLEAN DEFAULT FALSE,
    DueDate TIMESTAMPTZ,
    IsHabit BOOLEAN DEFAULT FALSE,
    Frequency VARCHAR(50),
    CustomDays VARCHAR(255),
    CurrentStreak INT DEFAULT 0,
    HighestStreak INT DEFAULT 0,
    LastCompletedAt TIMESTAMPTZ,
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

CREATE TABLE categories (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID REFERENCES "Users"("Id"),
  name text not null,
  description text,
  MonthlyLimit NUMERIC(10, 2),
  created_at timestamptz default now(),
  updated_at timestamptz default now()
);

CREATE TABLE Finances (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES "Users"("Id"),
    TransactionType TEXT NOT NULL, -- ingreso o gasto
    Amount NUMERIC(10, 2) NOT NULL,
    CategoryId UUID REFERENCES categories(id) ON DELETE SET NULL,
    PaymentMethod TEXT NOT NULL DEFAULT '',
    TransactionDate TIMESTAMPTZ DEFAULT NOW(),
    CreatedAt TIMESTAMPTZ DEFAULT NOW(),
    UpdatedAt TIMESTAMPTZ DEFAULT NOW()
);