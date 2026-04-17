namespace LifeAsAGame.Api.Models;

/// <summary>Daily quest / task owned by a user.</summary>
public class GameTask
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; }
    public SkillType SkillType { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>UTC calendar day this quest was intended for (daily bucket).</summary>
    public DateTime DueUtcDate { get; set; }
}
