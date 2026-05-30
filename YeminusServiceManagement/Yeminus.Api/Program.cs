using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Yeminus.Api.Middleware;
using Yeminus.Application.Extensions;
using Yeminus.Infrastructure.Extensions;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Infrastructure.Persistence;
using Yeminus.Infrastructure.Persistence.ConnectionFactories;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/yeminus-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Yeminus Service Management API",
        Version = "v1",
        Description = "API for managing service orders, clients and technicians."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------
// Database connectivity check (retries 5 times, 2s apart)
// ---------------------------------------------------------------
await ValidateDatabaseConnectionAsync(app.Services, app.Configuration);
await DatabaseSeeder.SeedAsync(
    app.Services.GetRequiredService<IDbConnectionFactory>(),
    app.Services.GetRequiredService<IPasswordHasher>(),
    app.Logger);

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Yeminus Service Management API v1");
        c.RoutePrefix = "swagger";
    });
}

// CORS must be after UseRouting and before UseAuthentication
// UseHttpsRedirection goes after CORS so preflight OPTIONS isn't redirected
app.UseRouting();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();

try
{
    Log.Information("Starting Yeminus Service Management API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ---------------------------------------------------------------
static async Task ValidateDatabaseConnectionAsync(IServiceProvider services, IConfiguration config)
{
    const int maxRetries = 5;
    const int delaySeconds = 2;

    var factory = services.GetRequiredService<IDbConnectionFactory>();
    var provider = config["DatabaseProvider"] ?? "PostgreSQL";

    Log.Information("Checking database connection ({Provider})...", provider);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var conn = factory.CreateConnection();
            conn.Open();
            Log.Information("Database connection OK ({Provider})", provider);
            return;
        }
        catch (Exception ex)
        {
            Log.Warning("Database connection attempt {Attempt}/{Max} failed: {Message}", attempt, maxRetries, ex.Message);

            if (attempt == maxRetries)
            {
                Log.Fatal(
                    "Cannot connect to the database after {Max} attempts.\n" +
                    "Provider : {Provider}\n" +
                    "Connection: {ConnStr}\n" +
                    "Make sure PostgreSQL is running and the connection string in appsettings.json is correct.",
                    maxRetries,
                    provider,
                    config.GetConnectionString("DefaultConnection") ?? config.GetConnectionString("OracleConnection"));
                throw new InvalidOperationException($"Database unavailable: {ex.Message}", ex);
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}
