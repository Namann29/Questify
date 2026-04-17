using LifeAsAGame.Api.Data;

namespace LifeAsAGame.Api.Infrastructure;

/// <summary>Reads Bearer tokens issued at login/register.</summary>
public static class HttpAuthExtensions
{
    public static int? GetUserId(this HttpContext http, GameDataStore store)
    {
        if (!http.Request.Headers.TryGetValue("Authorization", out var header))
            return null;

        var raw = header.ToString();
        const string prefix = "Bearer ";
        if (!raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return null;

        return store.GetUserIdByToken(raw[prefix.Length..].Trim());
    }
}
