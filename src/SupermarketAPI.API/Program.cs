using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Infrastructure.Data;
using SupermarketAPI.Infrastructure.Repositories;
using SupermarketAPI.Domain.Interfaces;
using SupermarketAPI.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Hangfire;
using Hangfire.SQLite;
using StackExchange.Redis;
using SupermarketAPI.Application.Services;
using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Services;
using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Notifications.Services;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Supermarket API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando o esquema Bearer. Ex: 'Bearer {token}'",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Health Checks
builder.Services.AddHealthChecks();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
    options.RejectionStatusCode = 429;
});

// EF Core - SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SupermarketDbContext>(options =>
    options.UseSqlite(connectionString));

// UnitOfWork / Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Hosted service de retenção a cada 3 dias
builder.Services.AddHostedService<DataRetentionHostedService>();

// Redis Cache
var redisConfig = builder.Configuration.GetSection("Redis").GetValue<string>("Configuration");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
});

// Hangfire (SQLite storage)
var hangfireConn = builder.Configuration.GetSection("Hangfire").GetValue<string>("ConnectionString");
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseSQLiteStorage(hangfireConn);
});
builder.Services.AddHangfireServer();

// App Services
builder.Services.AddSingleton<INormalizationService, NormalizationService>();
builder.Services.AddSingleton<IMatchingService, MatchingService>();
builder.Services.AddSingleton<ScraperOrchestrator>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<INotificationService, WhatsAppNotificationService>();
builder.Services.AddSingleton<IScraper, SupermarketAPI.Scrapers.Sites.AuchanScraper>();
builder.Services.AddSingleton<IScraper, SupermarketAPI.Scrapers.Sites.PingoDoceScraper>();
builder.Services.AddSingleton<IScraper, SupermarketAPI.Scrapers.Sites.ContinenteScraper>();
builder.Services.AddSingleton<IScraper, SupermarketAPI.Scrapers.Sites.LidlScraper>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<DataCleanupService>();
builder.Services.AddScoped<AnalyticsUpdateService>();

// Advanced Services
builder.Services.AddScoped<IAdvancedMatchingService, AdvancedMatchingService>();
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();
builder.Services.AddScoped<IKeywordAnalysisService, KeywordAnalysisService>();

// Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();

// Scraping Services
builder.Services.AddSingleton<IUserAgentRotationService, UserAgentRotationService>();
builder.Services.AddSingleton<IProxyRotationService, ProxyRotationService>();
builder.Services.AddSingleton<IScrapingConfigurationService, ScrapingConfigurationService>();
builder.Services.AddSingleton<IAntiBotDetectionService, AntiBotDetectionService>();

// Auth JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("Key")!);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("Default");
app.UseRateLimiter();

app.UseHangfireDashboard("/jobs");

app.MapHealthChecks("/health");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

// Garantir criação do banco na inicialização
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SupermarketDbContext>();
    Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
    db.Database.Migrate();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
