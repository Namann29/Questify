using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.DTOs;

public record RegisterRequest(string Username, string Password);
public record LoginRequest(string Username, string Password);

public record AuthResponse(string Token, UserSummaryDto User);

public record UserSummaryDto(
    int Id,
    string Username,
    int Level,
    int XpIntoCurrentLevel,
    int XpRequiredForNextLevel,
    int TotalXp,
    int Streak,
    int Coins,
    int Gems,
    string Theme,
    bool OnboardingDone,
    int StreakFreezeCount);

public record SkillDto(
    SkillType Type,
    string Label,
    int Level,
    int XpIntoCurrentLevel,
    int XpRequiredForNextLevel,
    int TotalXp,
    string Icon);

public record TaskDto(
    int Id,
    string Title,
    Difficulty Difficulty,
    SkillType SkillType,
    int XpReward,
    int CoinReward,
    bool Completed,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime DueUtcDate);

public record CreateTaskRequest(string Title, Difficulty Difficulty, SkillType SkillType);

public record CompleteTaskResultDto(
    TaskDto Task,
    UserSummaryDto User,
    IReadOnlyList<SkillDto> Skills,
    IReadOnlyList<ActivityEntryDto> NewActivities,
    bool LeveledUp,
    IReadOnlyList<string> Celebrations,
    IReadOnlyList<AchievementDto> NewAchievements,
    IReadOnlyList<QuestChainDto> UpdatedChains,
    int CoinsEarned);

public record ActivityEntryDto(
    ActivityKind Kind,
    string Message,
    int XpDelta,
    SkillType? SkillType,
    DateTime CreatedAtUtc);

public record DashboardDto(
    UserSummaryDto User,
    IReadOnlyList<SkillDto> Skills,
    IReadOnlyList<TaskDto> Tasks,
    IReadOnlyList<ActivityEntryDto> Activity,
    DifficultyRecommendationDto Difficulty,
    IReadOnlyList<AchievementDto> Achievements,
    IReadOnlyList<QuestChainDto> QuestChains,
    IReadOnlyList<HabitDto> Habits);

public record DifficultyRecommendationDto(
    string Suggested,
    string Reason,
    double CompletionRate,
    int CompletedLast7Days,
    int TotalActiveLast7Days);

public record AnalyticsDto(
    IReadOnlyList<string> WeekLabels,
    IReadOnlyList<int> XpPerDay,
    IReadOnlyDictionary<string, IReadOnlyList<int>> SkillXpPerDay,
    IReadOnlyList<int> CumulativeTotalXpPerDay);

// --- New Feature DTOs ---

public record AchievementDto(
    string Key,
    string Title,
    string Description,
    string Icon,
    DateTime UnlockedAtUtc);

public record QuestChainDto(
    int Id,
    string Title,
    string Description,
    bool Completed,
    IReadOnlyList<QuestChainStepDto> Steps,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);

public record QuestChainStepDto(
    int Order,
    string Title,
    SkillType SkillType,
    Difficulty Difficulty,
    int RequiredCount,
    int CompletedCount,
    bool IsComplete,
    int XpBonus);

public record ShopItemDto(
    string Key,
    string Name,
    string Description,
    int Cost,
    string Currency,
    string Category);

public record SpendRequest(string ItemKey);
public record ThemeRequest(string Theme);
public record OnboardingDoneRequest(bool Done);

// --- Habit DTOs ---

public record HabitDto(
    int Id,
    string Title,
    string Frequency,
    SkillType SkillType,
    int Streak,
    int BestStreak,
    bool IsCompletedInCurrentCycle,
    DateTime? LastCompletedAtUtc,
    DateTime CreatedAtUtc);

public record CreateHabitRequest(string Title, HabitFrequency Frequency, SkillType SkillType);
public record CompleteHabitResultDto(int XpAwarded, int StreakBonus, bool LeveledUp, UserSummaryDto User, HabitDto Habit);
