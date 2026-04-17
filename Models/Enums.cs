namespace LifeAsAGame.Api.Models;

/// <summary>Quest difficulty tiers — maps to fixed XP rewards.</summary>
public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2
}

/// <summary>Life skill tracks that quests can strengthen.</summary>
public enum SkillType
{
    Health = 0,
    Study = 1,
    Social = 2
}

/// <summary>Types of feed entries for the activity stream.</summary>
public enum ActivityKind
{
    TaskComplete,
    LevelUp,
    SkillLevelUp,
    Streak,
    QuestAdded
}
