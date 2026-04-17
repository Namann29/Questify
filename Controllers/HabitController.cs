using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.DTOs;
using LifeAsAGame.Api.Infrastructure;
using LifeAsAGame.Api.Models;
using LifeAsAGame.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LifeAsAGame.Api.Controllers;

[ApiController]
[Route("api/habits")]
public class HabitController : ControllerBase
{
    private readonly GameDataStore _store;
    private readonly HabitService _habitService;

    public HabitController(GameDataStore store, HabitService habitService)
    {
        _store = store;
        _habitService = habitService;
    }

    /// <summary>List all habits for the current user (resets expired cycles first).</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<HabitDto>> List()
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        _habitService.ResetExpiredCycles(id.Value);

        var habits = _store.GetHabits(id.Value).Select(DtoMapper.ToHabitDto).ToList();
        return Ok(habits);
    }

    /// <summary>Create a new habit.</summary>
    [HttpPost]
    public ActionResult<HabitDto> Create([FromBody] CreateHabitRequest body)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(body.Title))
            return BadRequest("Title is required.");

        var habit = new Habit
        {
            UserId = id.Value,
            Title = body.Title.Trim(),
            Frequency = body.Frequency,
            SkillType = body.SkillType,
        };

        _store.AddHabit(habit);

        _store.AddActivity(new ActivityEntry
        {
            UserId = id.Value,
            Kind = ActivityKind.QuestAdded,
            Message = $"New habit: {habit.Title} ({habit.Frequency})",
            XpDelta = 0,
            SkillType = habit.SkillType,
            CreatedAtUtc = DateTime.UtcNow
        });

        return Ok(DtoMapper.ToHabitDto(habit));
    }

    /// <summary>Complete a habit for the current cycle.</summary>
    [HttpPost("{habitId:int}/complete")]
    public ActionResult<CompleteHabitResultDto> Complete(int habitId)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var (xpAwarded, streakBonus, leveledUp) = _habitService.CompleteHabit(id.Value, habitId);

        if (xpAwarded == 0)
            return BadRequest("Habit not found or already completed this cycle.");

        var user = _store.GetUserById(id.Value)!;
        var habit = _store.GetHabit(id.Value, habitId)!;

        return Ok(new CompleteHabitResultDto(
            xpAwarded,
            streakBonus,
            leveledUp,
            DtoMapper.ToUserSummary(user),
            DtoMapper.ToHabitDto(habit)));
    }

    /// <summary>Delete a habit.</summary>
    [HttpDelete("{habitId:int}")]
    public ActionResult Delete(int habitId)
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var success = _store.DeleteHabit(id.Value, habitId);
        if (!success) return NotFound();

        return NoContent();
    }
}
