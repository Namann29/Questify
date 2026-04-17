using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>Checks and unlocks achievements based on player state changes.</summary>
public class AchievementService
{
    private readonly GameDataStore _store;

    public AchievementService(GameDataStore store) => _store = store;

    /// <summary>Run all achievement checks after a task completion. Returns newly unlocked achievements.</summary>
    public List<Achievement> CheckAfterTaskComplete(int userId, User user, GameTask task)
    {
        var unlocked = new List<Achievement>();

        // First Task
        if (!_store.HasAchievement(userId, AchievementDefs.FirstTask))
        {
            var completedCount = _store.GetTasks(userId).Count(t => t.Completed);
            if (completedCount >= 1)
                unlocked.Add(_store.AddAchievement(userId, AchievementDefs.FirstTask));
        }

        // 7-Day Streak
        if (!_store.HasAchievement(userId, AchievementDefs.SevenDayStreak) && user.Streak >= 7)
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.SevenDayStreak));

        // XP Master
        if (!_store.HasAchievement(userId, AchievementDefs.XpMaster) && user.TotalXp >= 500)
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.XpMaster));

        // Hard Mode Hero
        if (!_store.HasAchievement(userId, AchievementDefs.HardQuest) && task.Difficulty == Difficulty.Hard)
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.HardQuest));

        // All Skills leveled
        if (!_store.HasAchievement(userId, AchievementDefs.AllSkills))
        {
            var skills = _store.GetSkills(userId);
            if (skills.All(s => s.Level >= 2))
                unlocked.Add(_store.AddAchievement(userId, AchievementDefs.AllSkills));
        }

        // Level 5
        if (!_store.HasAchievement(userId, AchievementDefs.Level5) && user.Level >= 5)
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.Level5));

        // Level 10
        if (!_store.HasAchievement(userId, AchievementDefs.Level10) && user.Level >= 10)
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.Level10));

        // Coin Collector
        if (!_store.HasAchievement(userId, AchievementDefs.CoinCollector) && user.Coins >= 100)
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.CoinCollector));

        // Ten Tasks Day
        if (!_store.HasAchievement(userId, AchievementDefs.TenTasksDay))
        {
            var today = DateTime.UtcNow.Date;
            var todayCompleted = _store.GetTasks(userId)
                .Count(t => t.Completed && t.CompletedAtUtc?.Date == today);
            if (todayCompleted >= 10)
                unlocked.Add(_store.AddAchievement(userId, AchievementDefs.TenTasksDay));
        }

        return unlocked;
    }

    /// <summary>Check quest chain completion achievement.</summary>
    public List<Achievement> CheckAfterQuestChainComplete(int userId)
    {
        var unlocked = new List<Achievement>();
        if (!_store.HasAchievement(userId, AchievementDefs.QuestChainComplete))
            unlocked.Add(_store.AddAchievement(userId, AchievementDefs.QuestChainComplete));
        return unlocked;
    }
}
