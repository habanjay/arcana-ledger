using ArcanaLedger.Server.Apis;
using ArcanaLedger.Server.Dtos.Common;
using ArcanaLedger.Server.Dtos.Json;
using ArcanaLedger.Server.Middleware;
using ArcanaLedger.Server.Mocks;
using ArcanaLedger.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

// JWT bearer authentication: tokens are issued by JwtTokenService (Services/) from
// credentials verified against the mock user store (Mocks/).
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

// Mock data layer (Mocks/): a single shared in-memory store backs every resource service so
// state is consistent across requests. Local/demo use only.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<MockDataStore>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IAuthService, MockAuthService>();
builder.Services.AddSingleton<IPortfolioService, MockPortfolioService>();
builder.Services.AddSingleton<IBrokerageAccountService, MockBrokerageAccountService>();
builder.Services.AddSingleton<IInstrumentService, MockInstrumentService>();
builder.Services.AddSingleton<IWatchlistService, MockWatchlistService>();
builder.Services.AddSingleton<ITradeSetupService, MockTradeSetupService>();
builder.Services.AddSingleton<ITradeService, MockTradeService>();
builder.Services.AddSingleton<IPortfolioTransactionService, MockPortfolioTransactionService>();
builder.Services.AddSingleton<IPortfolioSnapshotService, MockPortfolioSnapshotService>();
builder.Services.AddSingleton<IPortfolioActivityService, MockPortfolioActivityService>();
builder.Services.AddSingleton<IDashboardService, MockDashboardService>();
builder.Services.AddSingleton<IValuationService, MockValuationService>();
builder.Services.AddSingleton<IPortfolioSettingsService, MockPortfolioSettingsService>();

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
