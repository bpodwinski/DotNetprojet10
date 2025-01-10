using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AuthService.Data;
using AuthService.Domain;
using AuthService.Repositories;
using AuthService.Services;
using Serilog;
using System.Reflection;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);
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

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("AuthService_.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthService",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Scheme = "Bearer",
        Name = "Authorization",
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

    // Get the XML comments file path
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configure DbContext and Identity
builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<UserDomain, IdentityRole<int>>()
    .AddEntityFrameworkStores<LocalDbContext>()
    .AddDefaultTokenProviders();

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

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                try
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<UserDomain>>();
                    var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();

                    logger.LogInformation("Token validation started.");

                    var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        logger.LogWarning("Token validation failed: User ID is missing.");
                        context.Fail("Invalid token: User ID is missing.");
                        return;
                    }

                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        logger.LogWarning("Token validation failed: User not found for ID {UserId}.", userId);
                        context.Fail("Invalid token: User not found.");
                        return;
                    }

                    if (context.SecurityToken is JsonWebToken token)
                    {
                        logger.LogInformation("Checking if token is revoked.");
                        var isRevoked = await jwtService.IsTokenRevoked(user, token.UnsafeToString());
                        if (isRevoked)
                        {
                            logger.LogWarning("Token validation failed: Token has been revoked.");
                            context.Fail("Token has been revoked.");
                            return;
                        }
                    }
                    else
                    {
                        logger.LogWarning("Token validation failed: SecurityToken is not a JwtSecurityToken.");
                        context.Fail("Invalid token: SecurityToken is invalid.");
                        return;
                    }

                    logger.LogInformation("Token validation succeeded.");
                }
                catch (Exception ex)
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during token validation.");
                    context.Fail($"An error occurred during token validation: {ex.Message}");
                }
            }
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });

    options.AddPolicy("User", policy =>
    {
        policy.RequireRole("User", "Admin");
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });
});

// Register repositories and services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

var app = builder.Build();

// Ensure database is migrated in Development mode
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbcontext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        dbcontext.Database.Migrate();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Serilog to record logs
app.UseSerilogRequestLogging();

// Call the role and administrator initialization method
await InitializeRolesAndAdminUserAsync(app.Services);

// Apply authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();


// Method for initializing roles and admin user
static async Task InitializeRolesAndAdminUserAsync(IServiceProvider serviceProvider)
{
    try
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserDomain>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "User" });
            await roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin" });

            var admins = await userManager.GetUsersInRoleAsync("Admin");

            if (admins.Count == 0)
            {
                var user = new UserDomain
                {
                    UserName = "admin",
                    FullName = "Super Admin",
                    Role = "Admin",
                };

                var result = await userManager.CreateAsync(user, "Ls7N0U7tmZ48!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, user.Role);
                    logger.LogInformation("Admin user created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing roles and admin user.");
    }
}
