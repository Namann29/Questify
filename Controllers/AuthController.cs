using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.DTOs;
using LifeAsAGame.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LifeAsAGame.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GameDataStore _store;

    public AuthController(GameDataStore store) => _store = store;

    /// <summary>Create a new player profile with default skills.</summary>
    [HttpPost("register")]
    public ActionResult<AuthResponse> Register([FromBody] RegisterRequest body)
    {
        if (string.IsNullOrWhiteSpace(body.Username) || body.Username.Length < 3)
            return BadRequest("Username must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(body.Password) || body.Password.Length < 4)
            return BadRequest("Password must be at least 4 characters.");

        if (_store.GetUserByUsername(body.Username) != null)
            return Conflict("Username already taken.");

        var user = _store.CreateUser(body.Username, PasswordHasher.Hash(body.Password));
        var token = Guid.NewGuid().ToString("N");
        _store.SetUserToken(user, token);

        return Ok(new AuthResponse(token, DtoMapper.ToUserSummary(user)));
    }

    /// <summary>Exchange credentials for a fresh bearer token.</summary>
    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest body)
    {
        var user = _store.GetUserByUsername(body.Username);
        if (user == null || !PasswordHasher.Verify(body.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password.");

        var token = Guid.NewGuid().ToString("N");
        _store.SetUserToken(user, token);

        return Ok(new AuthResponse(token, DtoMapper.ToUserSummary(user)));
    }

    /// <summary>Lightweight session check for the SPA.</summary>
    [HttpGet("me")]
    public ActionResult<UserSummaryDto> Me()
    {
        var id = HttpContext.GetUserId(_store);
        if (id is null) return Unauthorized();

        var user = _store.GetUserById(id.Value);
        return user == null ? Unauthorized() : Ok(DtoMapper.ToUserSummary(user));
    }
}
