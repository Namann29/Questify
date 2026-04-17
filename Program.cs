using System.Text.Json.Serialization;
using LifeAsAGame.Api.Data;
using LifeAsAGame.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameDataStore>();
builder.Services.AddSingleton<XPService>();
builder.Services.AddSingleton<DifficultyService>();
builder.Services.AddSingleton<CurrencyService>();
builder.Services.AddSingleton<AchievementService>();
builder.Services.AddSingleton<QuestChainService>();
builder.Services.AddSingleton<IntelligenceService>();
builder.Services.AddSingleton<HabitService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
    });

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// SPA-style fallback for static hosting alongside the API.
app.MapFallbackToFile("index.html");

app.Run();
