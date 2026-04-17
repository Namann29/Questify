using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.DTOs;
using LifeAsAGame.Api.Infrastructure;
using LifeAsAGame.Api.Models;
using LifeAsAGame.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeAsAGame.Api.Controllers;

/// <summary>Quest CRUD + completion pipeline (XP, skills, streak, feed, achievements, currency, chains).</summary>
[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly GameDataStore _store;
    private readonly XPService _xp;
    private readonly CurrencyService _currency;
    private readonly AchievementService _achievements;
    private readonly QuestChainService _questChains;

    public TaskController(GameDataStore store, XPService xp, CurrencyService currency, AchievementService achievements, QuestChainService questChains)
    {
        _store = store;
        _xp = xp;
        _currency = currency;
        _achievements = achievements;
        _questChains = questChains;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<TaskDto>> List()
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var list = _store.GetTasks(id.Value)
            .Select(t => DtoMapper.ToTaskDto(t, _xp.GetQuestXp(t.Difficulty), _currency.GetCoinReward(t.Difficulty)))
            .ToList();
        return Ok(list);
    }

    [HttpPost]
    public ActionResult<TaskDto> Create([FromBody] CreateTaskRequest body)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(body.Title))
            return BadRequest("Title is required.");

        var user = _store.GetUserById(id.Value);
        if (user == null) return Unauthorized();

        var task = new GameTask
        {
            UserId = id.Value,
            Title = body.Title.Trim(),
            Difficulty = body.Difficulty,
            SkillType = body.SkillType,
            DueUtcDate = DateTime.UtcNow.Date,
            Completed = false
        };

        _store.AddTask(task);

        var questMsg = $"New quest queued: {task.Title}";
        var stamp = DateTime.UtcNow;
        _store.AddActivity(new ActivityEntry
        {
            UserId = id.Value,
            Kind = ActivityKind.QuestAdded,
            Message = questMsg,
            XpDelta = 0,
            SkillType = task.SkillType,
            CreatedAtUtc = stamp
        });

        return Ok(DtoMapper.ToTaskDto(task, _xp.GetQuestXp(task.Difficulty), _currency.GetCoinReward(task.Difficulty)));
    }

    [HttpPost("{taskId:int}/complete")]
    public ActionResult<CompleteTaskResultDto> Complete(int taskId)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var user = _store.GetUserById(id.Value);
        if (user == null) return Unauthorized();

        var task = _store.GetTask(id.Value, taskId);
        if (task == null) return NotFound();
        if (task.Completed) return BadRequest("Quest already completed.");

        var xp = _xp.GetQuestXp(task.Difficulty);
        var coins = _currency.GetCoinReward(task.Difficulty);
        var skill = _store.GetSkill(id.Value, task.SkillType);
        if (skill == null) return BadRequest("Skill track missing.");

        var sink = new List<ActivityEntry>();
        _xp.AddCharacterXp(user, xp, id.Value, sink);

        var skillSink = new List<ActivityEntry>();
        _xp.AddSkillXp(skill, xp, id.Value, task.SkillType, skillSink);

        var streakSink = new List<ActivityEntry>();
        _xp.TouchStreak(user, streakSink);

        // Award coins
        _currency.AwardCoins(user, coins);

        var complete = new ActivityEntry
        {
            UserId = id.Value,
            Kind = ActivityKind.TaskComplete,
            Message = $"+{xp} XP · +{coins} coins from {task.SkillType} — {task.Title}",
            XpDelta = xp,
            SkillType = task.SkillType
        };

        task.Completed = true;
        task.CompletedAtUtc = DateTime.UtcNow;
        _store.UpdateTask(task);

        // Check achievements
        var newAchievements = _achievements.CheckAfterTaskComplete(id.Value, user, task);
        foreach (var a in newAchievements)
        {
            _currency.AwardGems(user, 5);
            sink.Add(new ActivityEntry
            {
                UserId = id.Value,
                Kind = ActivityKind.TaskComplete,
                Message = $"🏆 Achievement unlocked: {a.Title}!",
                XpDelta = 0,
                SkillType = null,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        // Update quest chain progress
        var updatedChains = _questChains.UpdateProgress(id.Value, task.SkillType, task.Difficulty);
        foreach (var chain in updatedChains.Where(c => c.Completed))
        {
            _store.AddActivity(new ActivityEntry
            {
                UserId = id.Value,
                Kind = ActivityKind.TaskComplete,
                Message = $"Quest chain complete: {chain.Title}! +{chain.Steps.Sum(s => s.XpBonus)} XP bonus",
                XpDelta = 0,
                SkillType = null,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        var ordered = new List<ActivityEntry>();
        ordered.AddRange(sink);
        ordered.AddRange(skillSink);
        ordered.AddRange(streakSink);
        ordered.Add(complete);

        static int FeedOrder(ActivityKind k) => k switch
        {
            ActivityKind.Streak => 0,
            ActivityKind.TaskComplete => 1,
            ActivityKind.SkillLevelUp => 2,
            ActivityKind.LevelUp => 3,
            _ => 1
        };
        ordered.Sort((a, b) => FeedOrder(a.Kind).CompareTo(FeedOrder(b.Kind)));
        var t0 = DateTime.UtcNow;
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].CreatedAtUtc = t0.AddMilliseconds(i * 25);

        foreach (var a in ordered)
            _store.AddActivity(a);

        var leveledUp = ordered.Any(a => a.Kind == ActivityKind.LevelUp);
        var celebrations = new List<string>();
        if (leveledUp) celebrations.Add("level_up");
        if (ordered.Any(a => a.Kind == ActivityKind.SkillLevelUp)) celebrations.Add("skill_up");
        if (newAchievements.Count > 0) celebrations.Add("achievement");
        if (updatedChains.Any(c => c.Completed)) celebrations.Add("chain_complete");

        var freshSkills = _store.GetSkills(id.Value).Select(DtoMapper.ToSkillDto).ToList();

        return Ok(new CompleteTaskResultDto(
            DtoMapper.ToTaskDto(task, xp, coins),
            DtoMapper.ToUserSummary(user),
            freshSkills,
            ordered.Select(DtoMapper.ToActivityDto).ToList(),
            leveledUp,
            celebrations,
            newAchievements.Select(DtoMapper.ToAchievementDto).ToList(),
            updatedChains.Select(DtoMapper.ToQuestChainDto).ToList(),
            coins));
    }
}
