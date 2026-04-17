namespace LifeAsAGame.Api.Models;

/// <summary>Single row in the live activity feed + analytics source.</summary>
public class ActivityEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ActivityKind Kind { get; set; }
    public string Message { get; set; } = string.Empty;
    public int XpDelta { get; set; }
    public SkillType? SkillType { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
