using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>Simple rule-based activity intelligence — no AI/ML, just data analysis.</summary>
public class IntelligenceService
{
    private readonly GameDataStore _store;

    public IntelligenceService(GameDataStore store) => _store = store;

    public IntelligenceDto GetInsights(int userId)
    {
        var acts = _store.GetRecentActivity(userId, 2000)
            .Where(a => a.Kind == ActivityKind.TaskComplete)
            .ToList();

        var tasks = _store.GetTasks(userId);
        var skills = _store.GetSkills(userId);

        // Most productive hour
        var productiveHour = acts
            .GroupBy(a => a.CreatedAtUtc.Hour)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault(-1);

        var productiveHourLabel = productiveHour >= 0
            ? productiveHour switch
            {
                >= 0 and < 12 => $"{(productiveHour == 0 ? 12 : productiveHour)} AM",
                12 => "12 PM",
                _ => $"{productiveHour - 12} PM"
            }
            : "Not enough data";

        // Skill distribution
        var skillCounts = acts
            .Where(a => a.SkillType != null)
            .GroupBy(a => a.SkillType!.Value)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var topSkill = skillCounts.OrderByDescending(k => k.Value).Select(k => k.Key).FirstOrDefault("None");

        // Completion rate last 14 days
        var cutoff = DateTime.UtcNow.AddDays(-14);
        var recentTasks = tasks.Where(t => t.CreatedAtUtc >= cutoff).ToList();
        var completedRecent = recentTasks.Count(t => t.Completed);
        var missedRecent = recentTasks.Count(t => !t.Completed && t.DueUtcDate < DateTime.UtcNow.Date);
        var total = completedRecent + missedRecent;
        var completionRate = total > 0 ? Math.Round((double)completedRecent / total * 100, 1) : 0;

        // Streak analysis
        var user = _store.GetUserById(userId);
        var streakDays = user?.Streak ?? 0;
        var longestPossibleStreak = user?.CreatedAtUtc != null
            ? (DateTime.UtcNow - user.CreatedAtUtc).Days + 1
            : 0;
        var streakEfficiency = longestPossibleStreak > 0
            ? Math.Round((double)streakDays / longestPossibleStreak * 100, 1)
            : 0;

        // Activity heatmap data (last 90 days)
        var heatmap = new List<HeatmapDayDto>();
        for (var i = 89; i >= 0; i--)
        {
            var day = DateTime.UtcNow.Date.AddDays(-i);
            var count = acts.Count(a => a.CreatedAtUtc.Date == day);
            heatmap.Add(new HeatmapDayDto(day.ToString("yyyy-MM-dd"), count));
        }

        // Recommendations
        var recommendations = new List<string>();
        if (completionRate < 40)
            recommendations.Add("Try setting easier goals to build momentum first.");
        if (completionRate > 80)
            recommendations.Add("You're on a roll! Try a harder challenge today 🔥");
        if (skillCounts.GetValueOrDefault("Health", 0) < skillCounts.GetValueOrDefault("Study", 0) / 2)
            recommendations.Add("You focus a lot on Study — consider adding Health tasks for balance.");
        if (skillCounts.GetValueOrDefault("Social", 0) < 3)
            recommendations.Add("Social tasks are rare. A small social win goes a long way!");
        if (streakDays >= 3)
            recommendations.Add($"Your {streakDays}-day streak is solid — don't break it!");

        return new IntelligenceDto(
            productiveHourLabel,
            topSkill,
            completionRate,
            streakDays,
            streakEfficiency,
            skillCounts,
            heatmap,
            recommendations
        );
    }
}

public record IntelligenceDto(
    string MostProductiveHour,
    string TopSkill,
    double CompletionRate14d,
    int CurrentStreak,
    double StreakEfficiency,
    Dictionary<string, int> SkillDistribution,
    List<HeatmapDayDto> Heatmap,
    List<string> Recommendations);

public record HeatmapDayDto(string Date, int Count);
