using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.Models;

namespace LifeAsAGame.Api.Services;

/// <summary>Manages quest chain creation, progression, and completion.</summary>
public class QuestChainService
{
    private readonly GameDataStore _store;

    public QuestChainService(GameDataStore store) => _store = store;

    /// <summary>Create a quest chain from a template for the user.</summary>
    public QuestChain CreateFromTemplate(int userId, QuestChainTemplate template)
    {
        var chain = new QuestChain
        {
            UserId = userId,
            Title = template.Title,
            Description = template.Description,
            Steps =
            [
                new QuestChainStep
                {
                    Order = 1,
                    Title = template.Title,
                    SkillType = template.SkillType,
                    Difficulty = template.Difficulty,
                    RequiredCount = template.RequiredCount,
                    CompletedCount = 0,
                    XpBonus = template.XpBonus
                }
            ]
        };
        return _store.AddQuestChain(chain);
    }

    /// <summary>Seed initial quest chains for a new user.</summary>
    public void SeedForNewUser(int userId)
    {
        foreach (var t in QuestChainTemplates.All.Take(2))
            CreateFromTemplate(userId, t);
    }

    /// <summary>Update quest chain progress based on a completed task.</summary>
    public List<QuestChain> UpdateProgress(int userId, SkillType skillType, Difficulty difficulty)
    {
        var chains = _store.GetQuestChains(userId);
        var updated = new List<QuestChain>();

        foreach (var chain in chains.Where(c => !c.Completed))
        {
            foreach (var step in chain.Steps.Where(s => !s.IsComplete))
            {
                if (step.SkillType == skillType && step.Difficulty <= difficulty)
                {
                    step.CompletedCount++;
                    if (step.IsComplete)
                    {
                        var allDone = chain.Steps.All(s => s.IsComplete);
                        if (allDone)
                        {
                            chain.Completed = true;
                            chain.CompletedAtUtc = DateTime.UtcNow;
                        }
                        updated.Add(chain);
                    }
                }
            }
        }

        return updated;
    }
}
