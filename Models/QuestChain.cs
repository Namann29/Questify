namespace LifeAsAGame.Api.Models;

/// <summary>A multi-step story quest chain with sequential steps.</summary>
public class QuestChain
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public List<QuestChainStep> Steps { get; set; } = [];
}

/// <summary>A single step within a quest chain.</summary>
public class QuestChainStep
{
    public int Id { get; set; }
    public int QuestChainId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public SkillType SkillType { get; set; }
    public Difficulty Difficulty { get; set; }
    public int RequiredCount { get; set; }
    public int CompletedCount { get; set; }
    public bool IsComplete => CompletedCount >= RequiredCount;
    public int XpBonus { get; set; }
}

/// <summary>Predefined quest chain templates.</summary>
public static class QuestChainTemplates
{
    public static readonly List<QuestChainTemplate> All =
    [
        new("Study Sprint", "Complete 3 Study tasks to unlock your next challenge.", SkillType.Study, Difficulty.Easy, 3, 75),
        new("Health Warrior", "Complete 2 Hard Health tasks to prove your endurance.", SkillType.Health, Difficulty.Hard, 2, 150),
        new("Social Butterfly", "Complete 4 Social tasks of any difficulty.", SkillType.Social, Difficulty.Easy, 4, 100),
        new("The Grand Quest", "Complete 5 tasks across all skills to become a legend.", SkillType.Study, Difficulty.Medium, 5, 250),
        new("Mind & Body", "Complete 2 Study + 2 Health tasks.", SkillType.Study, Difficulty.Medium, 4, 180),
    ];
}

public record QuestChainTemplate(string Title, string Description, SkillType SkillType, Difficulty Difficulty, int RequiredCount, int XpBonus);
