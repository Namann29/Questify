using LifeAsAGame.Api.DTOs;
using LifeAsAGame.Api.Models;
using LifeAsAGame.Api.Services;

namespace LifeAsAGame.Api.Infrastructure;

public static class DtoMapper
{
    public static UserSummaryDto ToUserSummary(User u) => new(
        u.Id,
        u.Username,
        u.Level,
        u.XpIntoCurrentLevel,
        XPService.XpRequiredForNextLevel(u.Level),
        u.TotalXp,
        u.Streak,
        u.Coins,
        u.Gems,
        u.Theme,
        u.OnboardingDone,
        u.StreakFreezeCount);

    public static SkillDto ToSkillDto(SkillState s) => new(
        s.Type,
        s.Type.ToString(),
        s.Level,
        s.XpIntoCurrentLevel,
        XPService.XpRequiredForNextLevel(s.Level),
        s.TotalXp,
        s.Type switch
        {
            SkillType.Health => "heart",
            SkillType.Study => "book",
            SkillType.Social => "users",
            _ => "sparkles"
        });

    public static TaskDto ToTaskDto(GameTask t, int xpReward, int coinReward) => new(
        t.Id,
        t.Title,
        t.Difficulty,
        t.SkillType,
        xpReward,
        coinReward,
        t.Completed,
        t.CreatedAtUtc,
        t.CompletedAtUtc,
        t.DueUtcDate);

    public static ActivityEntryDto ToActivityDto(ActivityEntry a) => new(
        a.Kind,
        a.Message,
        a.XpDelta,
        a.SkillType,
        a.CreatedAtUtc);

    public static AchievementDto ToAchievementDto(Achievement a) => new(
        a.Key,
        a.Title,
        a.Description,
        a.Icon,
        a.UnlockedAtUtc);

    public static QuestChainDto ToQuestChainDto(QuestChain c) => new(
        c.Id,
        c.Title,
        c.Description,
        c.Completed,
        c.Steps.Select(ToQuestChainStepDto).ToList(),
        c.CreatedAtUtc,
        c.CompletedAtUtc);

    public static QuestChainStepDto ToQuestChainStepDto(QuestChainStep s) => new(
        s.Order,
        s.Title,
        s.SkillType,
        s.Difficulty,
        s.RequiredCount,
        s.CompletedCount,
        s.IsComplete,
        s.XpBonus);

    public static HabitDto ToHabitDto(Habit h) => new(
        h.Id,
        h.Title,
        h.Frequency.ToString(),
        h.SkillType,
        h.Streak,
        h.BestStreak,
        h.IsCompletedInCurrentCycle,
        h.LastCompletedAtUtc,
        h.CreatedAtUtc);
}
