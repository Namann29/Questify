using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>Centralized XP awards and level-up loops for character + skills.</summary>
public class XPService
{
    public int GetQuestXp(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 10,
        Difficulty.Medium => 25,
        Difficulty.Hard => 50,
        _ => 10
    };

    /// <summary>XP threshold to advance from the current level to the next (100 × current level).</summary>
    public static int XpRequiredForNextLevel(int currentLevel) => 100 * Math.Max(1, currentLevel);

    /// <summary>Apply XP to the player character; emits level-up activity rows.</summary>
    public void AddCharacterXp(User user, int xp, int userId, List<ActivityEntry> sink)
    {
        if (xp <= 0) return;
        user.TotalXp += xp;
        user.XpIntoCurrentLevel += xp;

        var need = XpRequiredForNextLevel(user.Level);
        while (user.XpIntoCurrentLevel >= need)
        {
            user.XpIntoCurrentLevel -= need;
            user.Level++;
            need = XpRequiredForNextLevel(user.Level);
            sink.Add(new ActivityEntry
            {
                UserId = userId,
                Kind = ActivityKind.LevelUp,
                Message = $"Level Up! You are now level {user.Level}.",
                XpDelta = 0,
                SkillType = null,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }

    /// <summary>Apply XP to a single skill track; may emit skill level-up rows.</summary>
    public void AddSkillXp(SkillState skill, int xp, int userId, SkillType type, List<ActivityEntry> sink)
    {
        if (xp <= 0) return;
        skill.TotalXp += xp;
        skill.XpIntoCurrentLevel += xp;

        var need = XpRequiredForNextLevel(skill.Level);
        while (skill.XpIntoCurrentLevel >= need)
        {
            skill.XpIntoCurrentLevel -= need;
            skill.Level++;
            need = XpRequiredForNextLevel(skill.Level);
            var label = type.ToString();
            sink.Add(new ActivityEntry
            {
                UserId = userId,
                Kind = ActivityKind.SkillLevelUp,
                Message = $"{label} skill reached level {skill.Level}!",
                XpDelta = 0,
                SkillType = type,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }

    /// <summary>Update daily streak: first completion of a calendar UTC day advances the counter.</summary>
    public void TouchStreak(User user, List<ActivityEntry> sink)
    {
        var today = DateTime.UtcNow.Date;
        if (user.LastStreakUtcDate == today)
            return;

        if (user.LastStreakUtcDate is null)
        {
            user.Streak = 1;
        }
        else
        {
            var last = user.LastStreakUtcDate.Value;
            if (last == today.AddDays(-1))
                user.Streak++;
            else
                user.Streak = 1;
        }

        user.LastStreakUtcDate = today;
        var dayWord = user.Streak == 1 ? "day" : "days";
        sink.Add(new ActivityEntry
        {
            UserId = user.Id,
            Kind = ActivityKind.Streak,
            Message = $"Streak: {user.Streak} {dayWord} strong.",
            XpDelta = 0,
            SkillType = null,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
