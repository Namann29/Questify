namespace LifeAsAGame.Api.Models;

/// <summary>Player profile — core progression and streak state.</summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Bearer token for simple session auth (in-memory).</summary>
    public string? AuthToken { get; set; }

    public int Level { get; set; } = 1;
    /// <summary>XP accumulated toward the next character level.</summary>
    public int XpIntoCurrentLevel { get; set; }

    /// <summary>Lifetime XP for analytics / charts.</summary>
    public int TotalXp { get; set; }

    public int Streak { get; set; }
    /// <summary>UTC date of last day that counted toward streak maintenance.</summary>
    public DateTime? LastStreakUtcDate { get; set; }

    public int Coins { get; set; }
    public int Gems { get; set; }
    public string Theme { get; set; } = "darkNeon";
    public bool OnboardingDone { get; set; }
    public int StreakFreezeCount { get; set; }
    public DateTime? LastLoginUtcDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
