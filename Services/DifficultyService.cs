using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.DTOs;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>Rule-based difficulty nudges from recent quest completion patterns (no ML).</summary>
public class DifficultyService
{
    private readonly GameDataStore _store;

    public DifficultyService(GameDataStore store) => _store = store;

    public DifficultyRecommendationDto GetRecommendation(int userId)
    {
        var (tasks, _) = _store.GetWindow(userId, 7);
        var today = DateTime.UtcNow.Date;

        var relevant = tasks.Where(t => t.CreatedAtUtc >= today.AddDays(-7)).ToList();
        var completed = relevant.Count(t => t.Completed);
        var missed = relevant.Count(t => !t.Completed && t.DueUtcDate < today);
        var denominator = completed + missed;
        var rate = denominator == 0 ? 1.0 : (double)completed / denominator;

        string suggested;
        string reason;

        if (relevant.Count == 0)
        {
            suggested = "Medium";
            reason = "Start a few quests — we'll tune difficulty once we see your rhythm.";
        }
        else if (completed >= 12 && rate >= 0.75)
        {
            suggested = "Hard";
            reason = "You're clearing quests consistently — reach for harder rewards (+50 XP).";
        }
        else if (completed <= 3 || rate < 0.35)
        {
            suggested = "Easy";
            reason = "Momentum matters — smaller wins (+10 XP) will rebuild your streak.";
        }
        else
        {
            suggested = "Medium";
            reason = "Solid balance — medium quests (+25 XP) fit your current pace.";
        }

        return new DifficultyRecommendationDto(
            suggested,
            reason,
            Math.Round(rate, 2),
            completed,
            relevant.Count);
    }
}
