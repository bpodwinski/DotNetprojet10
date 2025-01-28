using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName;
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings{environment}.json", optional: true);

//  Load .env file if environment is not Docker
if (!string.Equals(environment, "Docker", StringComparison.OrdinalIgnoreCase))
{
    DotNetEnv.Env.Load();
}

var routesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Routes");
var ocelotRoutes = new JArray();

foreach (var file in Directory.GetFiles(routesFolder, "*.json"))
{
    var routeData = JArray.Parse(File.ReadAllText(file));
    foreach (var route in routeData)
    {
        ocelotRoutes.Add(route);
    }
}

var ocelotConfig = new JObject
{
    ["Routes"] = ocelotRoutes,
    ["GlobalConfiguration"] = new JObject
    {
        ["BaseUrl"] = "http://localhost:5000",
        ["AuthenticationOptions"] = new JObject
        {
            ["AuthenticationProviderKey"] = "Bearer"
        }
    }
};

var ocelotTempConfigPath = Path.Combine("tmp", "ocelot.generated.json");
File.WriteAllText(ocelotTempConfigPath, ocelotConfig.ToString());

builder.Configuration.AddJsonFile(ocelotTempConfigPath, optional: false, reloadOnChange: true);

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

builder.Services.AddOcelot();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();
