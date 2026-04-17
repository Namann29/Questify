using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.DTOs;
using LifeAsAGame.Api.Infrastructure;
using LifeAsAGame.Api.Models;
using LifeAsAGame.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeAsAGame.Api.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly GameDataStore _store;
    private readonly DifficultyService _difficulty;
    private readonly XPService _xp;
    private readonly CurrencyService _currency;
    private readonly IntelligenceService _intelligence;
    private readonly HabitService _habitService;

    public UserController(GameDataStore store, DifficultyService difficulty, XPService xp, CurrencyService currency, IntelligenceService intelligence, HabitService habitService)
    {
        _store = store;
        _difficulty = difficulty;
        _xp = xp;
        _currency = currency;
        _intelligence = intelligence;
        _habitService = habitService;
    }

    /// <summary>Full dashboard payload: profile, skills, quests, feed, difficulty hint, achievements, chains.</summary>
    [HttpGet("dashboard")]
    public ActionResult<DashboardDto> Dashboard()
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var user = _store.GetUserById(id.Value);
        if (user == null) return Unauthorized();

        // Track login
        _store.TouchLogin(user);

        var skills = _store.GetSkills(user.Id).Select(DtoMapper.ToSkillDto).ToList();
        var tasks = _store.GetTasks(user.Id)
            .Select(t => DtoMapper.ToTaskDto(t, _xp.GetQuestXp(t.Difficulty), _currency.GetCoinReward(t.Difficulty)))
            .ToList();
        var activity = _store.GetRecentActivity(user.Id).Select(DtoMapper.ToActivityDto).ToList();
        var diff = _difficulty.GetRecommendation(user.Id);
        var achievements = _store.GetAchievements(user.Id).Select(DtoMapper.ToAchievementDto).ToList();
        var chains = _store.GetQuestChains(user.Id).Select(DtoMapper.ToQuestChainDto).ToList();

        _habitService.ResetExpiredCycles(user.Id);
        var habits = _store.GetHabits(user.Id).Select(DtoMapper.ToHabitDto).ToList();

        return Ok(new DashboardDto(
            DtoMapper.ToUserSummary(user),
            skills,
            tasks,
            activity,
            diff,
            achievements,
            chains,
            habits));
    }

    /// <summary>Chart-friendly aggregates for the last 7 UTC days.</summary>
    [HttpGet("analytics")]
    public ActionResult<AnalyticsDto> Analytics()
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        if (_store.GetUserById(id.Value) == null) return Unauthorized();

        var acts = _store.GetRecentActivity(id.Value, 2000)
            .Where(a => a.Kind == ActivityKind.TaskComplete)
            .ToList();

        var today = DateTime.UtcNow.Date;
        var labels = new List<string>();
        var xpPerDay = new List<int>();
        var health = new List<int>();
        var study = new List<int>();
        var social = new List<int>();
        var cumulative = new List<int>();
        var running = 0;

        for (var i = 6; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            labels.Add(day.ToString("ddd"));
            var dayXp = acts.Where(a => a.CreatedAtUtc.Date == day).Sum(a => a.XpDelta);
            xpPerDay.Add(dayXp);
            running += dayXp;
            cumulative.Add(running);

            health.Add(acts.Where(a => a.CreatedAtUtc.Date == day && a.SkillType == SkillType.Health).Sum(a => a.XpDelta));
            study.Add(acts.Where(a => a.CreatedAtUtc.Date == day && a.SkillType == SkillType.Study).Sum(a => a.XpDelta));
            social.Add(acts.Where(a => a.CreatedAtUtc.Date == day && a.SkillType == SkillType.Social).Sum(a => a.XpDelta));
        }

        var skillMap = new Dictionary<string, IReadOnlyList<int>>
        {
            ["Health"] = health,
            ["Study"] = study,
            ["Social"] = social
        };

        return Ok(new AnalyticsDto(labels, xpPerDay, skillMap, cumulative));
    }

    /// <summary>Activity intelligence insights.</summary>
    [HttpGet("intelligence")]
    public ActionResult<IntelligenceDto> Intelligence()
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();
        return Ok(_intelligence.GetInsights(id.Value));
    }

    /// <summary>Get available shop items.</summary>
    [HttpGet("shop")]
    public ActionResult<IReadOnlyList<ShopItemDto>> Shop()
    {
        var items = new List<ShopItemDto>
        {
            new("theme_minimal", "Minimal Light Theme", "A clean, minimal light theme", 50, "coins", "theme"),
            new("theme_cyberpunk", "Cyberpunk Theme", "Neon-soaked cyberpunk aesthetic", 50, "coins", "theme"),
            new("xp_boost", "2x XP Boost", "Double XP on your next task completion", 30, "coins", "boost"),
            new("streak_freeze", "Streak Freeze", "Protect your streak for one missed day", 10, "gems", "protection"),
            new("gem_pack", "Gem Pack (5)", "5 premium gems for special purchases", 25, "coins", "currency"),
        };
        return Ok(items);
    }

    /// <summary>Purchase an item from the shop.</summary>
    [HttpPost("shop/buy")]
    public ActionResult<UserSummaryDto> BuyItem([FromBody] SpendRequest body)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var user = _store.GetUserById(id.Value);
        if (user == null) return Unauthorized();

        var success = body.ItemKey switch
        {
            "theme_minimal" => _currency.BuyTheme(user, "minimalLight"),
            "theme_cyberpunk" => _currency.BuyTheme(user, "cyberpunk"),
            "xp_boost" => _currency.BuyXpBoost(user),
            "streak_freeze" => _currency.BuyStreakFreeze(user),
            "gem_pack" => SpendCoinsAndAwardGems(user, 25, 5),
            _ => false
        };

        if (!success) return BadRequest("Not enough currency or invalid item.");

        return Ok(DtoMapper.ToUserSummary(user));
    }

    /// <summary>Switch active theme.</summary>
    [HttpPost("theme")]
    public ActionResult<UserSummaryDto> SetTheme([FromBody] ThemeRequest body)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var user = _store.GetUserById(id.Value);
        if (user == null) return Unauthorized();

        var validThemes = new[] { "darkNeon", "minimalLight", "cyberpunk" };
        if (!validThemes.Contains(body.Theme))
            return BadRequest("Invalid theme. Valid: darkNeon, minimalLight, cyberpunk");

        user.Theme = body.Theme;
        return Ok(DtoMapper.ToUserSummary(user));
    }

    /// <summary>Mark onboarding as done.</summary>
    [HttpPost("onboarding")]
    public ActionResult<UserSummaryDto> CompleteOnboarding([FromBody] OnboardingDoneRequest body)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var user = _store.GetUserById(id.Value);
        if (user == null) return Unauthorized();

        user.OnboardingDone = body.Done;
        return Ok(DtoMapper.ToUserSummary(user));
    }

    private bool SpendCoinsAndAwardGems(User user, int coinCost, int gems)
    {
        if (!_currency.SpendCoins(user, coinCost)) return false;
        _currency.AwardGems(user, gems);
        return true;
    }
}
