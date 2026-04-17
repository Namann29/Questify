namespace LifeAsAGame.Api.Models;

/// <summary>Badge unlocked based on player actions.</summary>
public class Achievement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public DateTime UnlockedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Static definition of all possible achievements.</summary>
public static class AchievementDefs
{
    public const string FirstTask = "first_task";
    public const string SevenDayStreak = "seven_day_streak";
    public const string XpMaster = "xp_master";
    public const string HardQuest = "hard_quest";
    public const string AllSkills = "all_skills";
    public const string Level5 = "level_5";
    public const string Level10 = "level_10";
    public const string CoinCollector = "coin_collector";
    public const string QuestChainComplete = "quest_chain_complete";
    public const string TenTasksDay = "ten_tasks_day";

    public static readonly Dictionary<string, (string Title, string Desc, string Icon)> All = new()
    {
        [FirstTask] = ("First Task Completed", "You completed your very first quest!", "trophy"),
        [SevenDayStreak] = ("7-Day Streak", "You kept the fire alive for 7 days straight!", "flame"),
        [XpMaster] = ("XP Master", "Accumulated 500+ lifetime XP", "star"),
        [HardQuest] = ("Hard Mode Hero", "Completed a Hard difficulty quest", "shield"),
        [AllSkills] = ("Well Rounded", "Leveled up all three skill tracks", "crown"),
        [Level5] = ("Rising Star", "Reached character level 5", "arrow-up"),
        [Level10] = ("Veteran Player", "Reached character level 10", "gem"),
        [CoinCollector] = ("Coin Collector", "Earned 100 coins total", "coins"),
        [QuestChainComplete] = ("Story Mode Champion", "Completed an entire quest chain", "book-open"),
        [TenTasksDay] = ("Productivity Beast", "Completed 10 tasks in a single day", "zap"),
    };
}
