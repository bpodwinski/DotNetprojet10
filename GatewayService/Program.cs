using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Charger les routes à partir de plusieurs fichiers
var routesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Routes");
var ocelotRoutes = new JArray();

foreach (var file in Directory.GetFiles(routesFolder, "*.json"))
{
    var routeData = JArray.Parse(File.ReadAllText(file)); // Lire le contenu du fichier JSON comme tableau
    foreach (var route in routeData)
    {
        ocelotRoutes.Add(route); // Ajouter chaque route au tableau principal
    }
}

// Créer la configuration Ocelot
var ocelotConfig = new JObject
{
    ["Routes"] = ocelotRoutes,
    ["GlobalConfiguration"] = new JObject
    {
        ["BaseUrl"] = "http://localhost:5000"
    }
};

// Sauvegarder temporairement la configuration combinée pour Ocelot
var ocelotTempConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "ocelot.generated.json");
File.WriteAllText(ocelotTempConfigPath, ocelotConfig.ToString());

// Utiliser le fichier temporaire pour configurer Ocelot
builder.Configuration.AddJsonFile(ocelotTempConfigPath, optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

var app = builder.Build();

await app.UseOcelot();

app.Run();
