using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReportService;
using ReportService.Repositories;
using ReportService.Services;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load configuration files
ConfigurationManager configuration = builder.Configuration;

var environment = builder.Environment.EnvironmentName;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings{environment}.json", optional: true);

//  Load .env file if environment is not Docker
if (!string.Equals(environment, "Docker", StringComparison.OrdinalIgnoreCase))
{
    DotNetEnv.Env.Load();
}

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("ReportService_.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ReportService API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Include XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configure JWT Authentication
IdentityModelEventSource.ShowPII = true;

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secretKey))
{
    throw new Exception("JWT configuration is incomplete. Ensure JWT_ISSUER, JWT_AUDIENCE, and JWT_SECRET_KEY are set in environment variables.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure role-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("User", policy =>
    {
        policy.RequireRole("User", "Admin");
        policy.RequireAuthenticatedUser();
    });
});

// Register repositories and services
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IReportService, ReportService.Services.ReportService>();

// Register ML.NET Trigger Analyzer as a singleton
builder.Services.AddSingleton<ML>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ML>>();
    string modelPath = "Models/TriggerModel.zip";
    return new ML(modelPath, logger);
});

// Register ModelInitializer
builder.Services.AddSingleton<ModelInitializer>();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Apply Serilog request logging
app.UseSerilogRequestLogging();

// Initialize ML.NET model
using (var scope = app.Services.CreateScope())
{
    var modelInitializer = scope.ServiceProvider.GetRequiredService<ModelInitializer>();
    modelInitializer.Initialize();
}

// Apply middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
