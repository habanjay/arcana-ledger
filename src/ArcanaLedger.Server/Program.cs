using ArcanaLedger.Server.Apis;
using ArcanaLedger.Server.Database;
using ArcanaLedger.Server.Dtos.Common;
using ArcanaLedger.Server.Dtos.Json;
using ArcanaLedger.Server.Middleware;
using ArcanaLedger.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

if (args.Contains("--apply-database-script", StringComparer.Ordinal))
{
    var scriptConnectionString = builder.Configuration.GetConnectionString("ArcanaLedger")
        ?? throw new InvalidOperationException(
            "Set ConnectionStrings:ArcanaLedger before applying the database script.");
    var scriptPath = Path.Combine(AppContext.BaseDirectory, "Database", "schema.sql");
    var script = await File.ReadAllTextAsync(scriptPath);

    await using var connection = new NpgsqlConnection(scriptConnectionString);
    await connection.OpenAsync();
    await using var command = new NpgsqlCommand(script, connection);
    await command.ExecuteNonQueryAsync();

    Console.WriteLine("Arcana Ledger database schema applied successfully.");
    return;
}

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new UtcDateTimeOffsetJsonConverter());
    options.SerializerOptions.Converters.Add(new NullableUtcDateTimeOffsetJsonConverter());
});

// JWT bearer authentication uses credentials verified against PostgreSQL app_users.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSection["SigningKey"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

// Rate limit sensitive, unauthenticated endpoints (register/login) against credential-stuffing
// and brute-force attempts.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("sensitive", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var connectionString = builder.Configuration.GetConnectionString("ArcanaLedger")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings:ArcanaLedger before starting Arcana Ledger Server.");
builder.Services.AddSingleton(NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IAuthService, PostgresAuthService>();
builder.Services.AddSingleton<IPortfolioService, PostgresPortfolioService>();
builder.Services.AddSingleton<IBrokerageAccountService, PostgresBrokerageAccountService>();
builder.Services.AddSingleton<IInstrumentService, PostgresInstrumentService>();
builder.Services.AddSingleton<IWatchlistService, PostgresWatchlistService>();
builder.Services.AddSingleton<ITradeSetupService, PostgresTradeSetupService>();
builder.Services.AddSingleton<ITradeService, PostgresTradeService>();
builder.Services.AddSingleton<IPortfolioTransactionService, PostgresPortfolioTransactionService>();
builder.Services.AddSingleton<IPortfolioSnapshotService, PostgresPortfolioSnapshotService>();
builder.Services.AddSingleton<IPortfolioActivityService, PostgresPortfolioActivityService>();
builder.Services.AddSingleton<IDashboardService, PostgresDashboardService>();
builder.Services.AddSingleton<IValuationService, PostgresValuationService>();
builder.Services.AddSingleton<IPortfolioSettingsService, PostgresPortfolioSettingsService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;
    if (response.HasStarted || response.ContentLength is > 0)
    {
        return;
    }

    var problem = new ApiErrorResponse(
        Type: $"https://arcanaledger.dev/errors/http-{response.StatusCode}",
        Title: ReasonPhrases.GetReasonPhrase(response.StatusCode),
        Status: response.StatusCode);
    await response.WriteAsJsonAsync(problem);
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapGet("weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapArcanaLedgerApi();

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
