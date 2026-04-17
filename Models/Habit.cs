namespace LifeAsAGame.Api.Models;

/// <summary>Repeatable habit with frequency tracking and streaks.</summary>
public class Habit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
    public SkillType SkillType { get; set; } = SkillType.Study;
    public int Streak { get; set; }
    public int BestStreak { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastCompletedAtUtc { get; set; }
    public bool IsCompletedInCurrentCycle { get; set; }
}

public enum HabitFrequency
{
    Daily,
    Weekly
}
