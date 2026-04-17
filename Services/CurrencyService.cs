using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>In-game currency: coins earned from tasks, gems from achievements. Shop logic for spending.</summary>
public class CurrencyService
{
    private readonly GameDataStore _store;

    public CurrencyService(GameDataStore store) => _store = store;

    public int GetCoinReward(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => 5,
        Difficulty.Medium => 12,
        Difficulty.Hard => 25,
        _ => 5
    };

    public void AwardCoins(User user, int coins)
    {
        user.Coins += coins;
    }

    public void AwardGems(User user, int gems)
    {
        user.Gems += gems;
    }

    /// <summary>Attempt to spend coins. Returns true if successful.</summary>
    public bool SpendCoins(User user, int amount)
    {
        if (user.Coins < amount) return false;
        user.Coins -= amount;
        return true;
    }

    /// <summary>Attempt to spend gems. Returns true if successful.</summary>
    public bool SpendGems(User user, int amount)
    {
        if (user.Gems < amount) return false;
        user.Gems -= amount;
        return true;
    }

    /// <summary>Activate a streak freeze (costs 10 gems). Returns true if activated.</summary>
    public bool BuyStreakFreeze(User user)
    {
        if (!SpendGems(user, 10)) return false;
        user.StreakFreezeCount++;
        return true;
    }

    /// <summary>Buy a theme (costs 50 coins). Returns true if successful.</summary>
    public bool BuyTheme(User user, string themeKey)
    {
        var cost = 50;
        if (!SpendCoins(user, cost)) return false;
        user.Theme = themeKey;
        return true;
    }

    /// <summary>Buy XP boost (2x XP for next task, costs 30 coins). Returns true if successful.</summary>
    public bool BuyXpBoost(User user)
    {
        if (!SpendCoins(user, 30)) return false;
        return true;
    }
}
