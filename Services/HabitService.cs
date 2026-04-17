using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>Handles habit cycle resets, completion, streak logic, and XP rewards.</summary>
public class HabitService
{
    private readonly GameDataStore _store;
    private readonly XPService _xp;

    public HabitService(GameDataStore store, XPService xp)
    {
        _store = store;
        _xp = xp;
    }

    /// <summary>Reset habits whose cycle has expired (daily at midnight, weekly at Monday start).</summary>
    public void ResetExpiredCycles(int userId)
    {
        var habits = _store.GetHabits(userId);
        var now = DateTime.UtcNow;

        foreach (var h in habits)
        {
            if (!h.IsCompletedInCurrentCycle || h.LastCompletedAtUtc == null) continue;

            var cycleStart = h.Frequency switch
            {
                HabitFrequency.Daily => now.Date,
                HabitFrequency.Weekly => GetStartOfWeek(now),
                _ => now.Date
            };

            if (h.LastCompletedAtUtc.Value < cycleStart)
            {
                // Cycle expired — check if streak should break
                var prevCycleStart = h.Frequency == HabitFrequency.Weekly
                    ? GetStartOfWeek(now).AddDays(-7)
                    : now.Date.AddDays(-1);

                if (h.LastCompletedAtUtc.Value < prevCycleStart)
                {
                    // Missed a full cycle — streak broken
                    h.Streak = 0;
                }

                h.IsCompletedInCurrentCycle = false;
            }
        }
    }

    /// <summary>Complete a habit for the current cycle. Returns XP awarded (0 if already completed).</summary>
    public (int xpAwarded, int streakBonus, bool leveledUp) CompleteHabit(int userId, int habitId)
    {
        ResetExpiredCycles(userId);

        var habit = _store.GetHabit(userId, habitId);
        if (habit == null) return (0, 0, false);

        // Prevent double completion in same cycle
        if (habit.IsCompletedInCurrentCycle) return (0, 0, false);

        var user = _store.GetUserById(userId);
        if (user == null) return (0, 0, false);

        // Base XP by frequency
        var baseXp = habit.Frequency switch
        {
            HabitFrequency.Daily => 10,
            HabitFrequency.Weekly => 25,
            _ => 10
        };

        // Streak bonus
        habit.Streak++;
        if (habit.Streak > habit.BestStreak) habit.BestStreak = habit.Streak;

        var streakBonus = habit.Streak switch
        {
            >= 7 => 15,
            >= 3 => 5,
            _ => 0
        };

        var totalXp = baseXp + streakBonus;

        // Award XP
        var sink = new List<ActivityEntry>();
        _xp.AddCharacterXp(user, totalXp, userId, sink);

        // Award skill XP
        var skill = _store.GetSkill(userId, habit.SkillType);
        if (skill != null)
        {
            var skillSink = new List<ActivityEntry>();
            _xp.AddSkillXp(skill, totalXp, userId, habit.SkillType, skillSink);
        }

        // Touch streak
        var streakSink = new List<ActivityEntry>();
        _xp.TouchStreak(user, streakSink);

        // Mark completed
        habit.IsCompletedInCurrentCycle = true;
        habit.LastCompletedAtUtc = DateTime.UtcNow;

        // Log activity
        var bonusText = streakBonus > 0 ? $" (+{streakBonus} streak bonus)" : "";
        _store.AddActivity(new ActivityEntry
        {
            UserId = userId,
            Kind = ActivityKind.TaskComplete,
            Message = $"+{totalXp} XP from habit: {habit.Title}{bonusText}",
            XpDelta = totalXp,
            SkillType = habit.SkillType,
            CreatedAtUtc = DateTime.UtcNow
        });

        foreach (var a in sink.Concat(streakSink))
            _store.AddActivity(a);

        var leveledUp = sink.Any(a => a.Kind == ActivityKind.LevelUp);

        return (totalXp, streakBonus, leveledUp);
    }

    private static DateTime GetStartOfWeek(DateTime dt)
    {
        var diff = (7 + (dt.DayOfWeek - DayOfWeek.Monday) % 7) % 7;
        return dt.Date.AddDays(-diff);
    }
}
