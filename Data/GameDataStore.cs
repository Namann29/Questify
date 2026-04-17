using System.Collections.Concurrent;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Data;

/// <summary>Thread-safe in-memory persistence. Swap for EF/SQLite when NuGet is available.</summary>
public class GameDataStore
{
    private int _nextUserId = 1;
    private int _nextSkillId = 1;
    private int _nextTaskId = 1;
    private int _nextActivityId = 1;
    private int _nextAchievementId = 1;
    private int _nextQuestChainId = 1;
    private int _nextQuestChainStepId = 1;
    private int _nextHabitId = 1;

    private readonly ConcurrentDictionary<int, User> _users = new();
    private readonly ConcurrentDictionary<string, int> _tokenToUserId = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<SkillType, SkillState>> _skills = new();
    private readonly ConcurrentDictionary<int, ConcurrentBag<GameTask>> _tasksByUser = new();
    private readonly ConcurrentDictionary<int, ConcurrentBag<ActivityEntry>> _activityByUser = new();
    private readonly ConcurrentDictionary<int, ConcurrentBag<Achievement>> _achievementsByUser = new();
    private readonly ConcurrentDictionary<int, ConcurrentBag<QuestChain>> _questChainsByUser = new();
    private readonly ConcurrentDictionary<int, ConcurrentBag<Habit>> _habitsByUser = new();

    public User? GetUserById(int id) => _users.GetValueOrDefault(id);

    public User? GetUserByUsername(string username)
    {
        var key = username.Trim().ToLowerInvariant();
        return _users.Values.FirstOrDefault(u => u.Username.ToLowerInvariant() == key);
    }

    public int? GetUserIdByToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        return _tokenToUserId.GetValueOrDefault(token.Trim());
    }

    public void SetUserToken(User user, string token)
    {
        if (user.AuthToken is { Length: > 0 } old)
            _tokenToUserId.TryRemove(old, out _);

        user.AuthToken = token;
        _tokenToUserId[token] = user.Id;
    }

    public User CreateUser(string username, string passwordHash)
    {
        var user = new User
        {
            Id = Interlocked.Increment(ref _nextUserId),
            Username = username.Trim(),
            PasswordHash = passwordHash
        };
        _users[user.Id] = user;

        var bag = new ConcurrentDictionary<SkillType, SkillState>();
        foreach (SkillType t in Enum.GetValues<SkillType>())
        {
            var s = new SkillState
            {
                Id = Interlocked.Increment(ref _nextSkillId),
                UserId = user.Id,
                Type = t,
                Level = 1,
                XpIntoCurrentLevel = 0,
                TotalXp = 0
            };
            bag[t] = s;
        }
        _skills[user.Id] = bag;
        _tasksByUser[user.Id] = new ConcurrentBag<GameTask>();
        _activityByUser[user.Id] = new ConcurrentBag<ActivityEntry>();
        _achievementsByUser[user.Id] = new ConcurrentBag<Achievement>();
        _questChainsByUser[user.Id] = new ConcurrentBag<QuestChain>();
        _habitsByUser[user.Id] = new ConcurrentBag<Habit>();

        return user;
    }

    public IReadOnlyList<SkillState> GetSkills(int userId) =>
        _skills.TryGetValue(userId, out var dict)
            ? dict.Values.OrderBy(s => s.Type).ToList()
            : Array.Empty<SkillState>();

    public SkillState? GetSkill(int userId, SkillType type) =>
        _skills.TryGetValue(userId, out var dict) ? dict.GetValueOrDefault(type) : null;

    public IReadOnlyList<GameTask> GetTasks(int userId) =>
        _tasksByUser.TryGetValue(userId, out var bag) ? bag.OrderByDescending(t => t.CreatedAtUtc).ToList() : [];

    public GameTask? GetTask(int userId, int taskId)
    {
        if (!_tasksByUser.TryGetValue(userId, out var bag)) return null;
        return bag.FirstOrDefault(t => t.Id == taskId);
    }

    public GameTask AddTask(GameTask task)
    {
        task.Id = Interlocked.Increment(ref _nextTaskId);
        _tasksByUser.GetOrAdd(task.UserId, _ => new ConcurrentBag<GameTask>()).Add(task);
        return task;
    }

    public void UpdateTask(GameTask task)
    {
        /* In-memory bag is immutable per item; task is reference type — mutations visible. */
    }

    public IReadOnlyList<ActivityEntry> GetRecentActivity(int userId, int take = 30)
    {
        if (!_activityByUser.TryGetValue(userId, out var bag)) return [];
        return bag.OrderByDescending(a => a.CreatedAtUtc).Take(take).ToList();
    }

    public ActivityEntry AddActivity(ActivityEntry entry)
    {
        entry.Id = Interlocked.Increment(ref _nextActivityId);
        _activityByUser.GetOrAdd(entry.UserId, _ => new ConcurrentBag<ActivityEntry>()).Add(entry);
        return entry;
    }

    /// <summary>Tasks and activity in the rolling window for difficulty heuristics.</summary>
    public (List<GameTask> tasks, List<ActivityEntry> activity) GetWindow(int userId, int days)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var tasks = GetTasks(userId).Where(t => t.CreatedAtUtc >= cutoff).ToList();
        var acts = _activityByUser.TryGetValue(userId, out var bag)
            ? bag.Where(a => a.CreatedAtUtc >= cutoff).ToList()
            : [];
        return (tasks, acts);
    }

    // --- Achievements ---

    public IReadOnlyList<Achievement> GetAchievements(int userId) =>
        _achievementsByUser.TryGetValue(userId, out var bag)
            ? bag.OrderByDescending(a => a.UnlockedAtUtc).ToList()
            : [];

    public bool HasAchievement(int userId, string key) =>
        _achievementsByUser.TryGetValue(userId, out var bag) &&
        bag.Any(a => a.Key == key);

    public Achievement AddAchievement(int userId, string key)
    {
        var def = AchievementDefs.All.GetValueOrDefault(key);
        var a = new Achievement
        {
            Id = Interlocked.Increment(ref _nextAchievementId),
            UserId = userId,
            Key = key,
            Title = def.Title,
            Description = def.Desc,
            Icon = def.Icon,
            UnlockedAtUtc = DateTime.UtcNow
        };
        _achievementsByUser.GetOrAdd(userId, _ => new ConcurrentBag<Achievement>()).Add(a);
        return a;
    }

    // --- Quest Chains ---

    public IReadOnlyList<QuestChain> GetQuestChains(int userId) =>
        _questChainsByUser.TryGetValue(userId, out var bag)
            ? bag.OrderByDescending(c => c.CreatedAtUtc).ToList()
            : [];

    public QuestChain? GetQuestChain(int userId, int chainId) =>
        _questChainsByUser.TryGetValue(userId, out var bag)
            ? bag.FirstOrDefault(c => c.Id == chainId)
            : null;

    public QuestChain AddQuestChain(QuestChain chain)
    {
        chain.Id = Interlocked.Increment(ref _nextQuestChainId);
        foreach (var step in chain.Steps)
            step.Id = Interlocked.Increment(ref _nextQuestChainStepId);
        _questChainsByUser.GetOrAdd(chain.UserId, _ => new ConcurrentBag<QuestChain>()).Add(chain);
        return chain;
    }

    // --- Login tracking ---

    public void TouchLogin(User user)
    {
        var today = DateTime.UtcNow.Date;
        if (user.LastLoginUtcDate != today)
        {
            user.LastLoginUtcDate = today;
        }
    }

    // --- Habits ---

    public IReadOnlyList<Habit> GetHabits(int userId) =>
        _habitsByUser.TryGetValue(userId, out var bag)
            ? bag.OrderByDescending(h => h.CreatedAtUtc).ToList()
            : [];

    public Habit? GetHabit(int userId, int habitId) =>
        _habitsByUser.TryGetValue(userId, out var bag)
            ? bag.FirstOrDefault(h => h.Id == habitId)
            : null;

    public Habit AddHabit(Habit habit)
    {
        habit.Id = Interlocked.Increment(ref _nextHabitId);
        _habitsByUser.GetOrAdd(habit.UserId, _ => new ConcurrentBag<Habit>()).Add(habit);
        return habit;
    }

    public bool DeleteHabit(int userId, int habitId)
    {
        if (!_habitsByUser.TryGetValue(userId, out var bag)) return false;
        var habit = bag.FirstOrDefault(h => h.Id == habitId);
        if (habit == null) return false;
        // ConcurrentBag doesn't support removal; replace the bag
        var remaining = bag.Where(h => h.Id != habitId).ToList();
        _habitsByUser[userId] = new ConcurrentBag<Habit>(remaining);
        return true;
    }
}
