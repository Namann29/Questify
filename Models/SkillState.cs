namespace LifeAsAGame.Api.Models;

/// <summary>Per-user skill progression (Health, Study, Social).</summary>
public class SkillState
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public SkillType Type { get; set; }
    public int Level { get; set; } = 1;
    public int XpIntoCurrentLevel { get; set; }
    public int TotalXp { get; set; }
}
