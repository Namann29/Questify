namespace LifeAsAGame.Api.Models;

/// <summary>Per-user customization and preferences.</summary>
public class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Theme { get; set; } = "darkNeon";
    public bool OnboardingDone { get; set; }
    public int StreakFreezeCount { get; set; }
    public DateTime? LastLoginUtcDate { get; set; }
}
