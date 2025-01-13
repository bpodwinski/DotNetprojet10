using Microsoft.ML;
using Microsoft.ML.Data;
using System.Diagnostics;
using ReportService;

public class ML
{
    private readonly PredictionEngine<ModelInput, TriggerPrediction> _predictionEngine;
    private readonly string _modelPath;
    private readonly ILogger<ML> _logger;

    public ML(string modelPath, ILogger<ML> logger)
    {
        _modelPath = modelPath;
        _logger = logger;

        if (!File.Exists(modelPath))
        {
            _logger.LogWarning($"Le modèle ML.NET est introuvable au chemin : {modelPath}. Entraînement en cours...");

            var stopwatch = Stopwatch.StartNew();
            TrainModel();
            stopwatch.Stop();

            _logger.LogInformation($"Entraînement terminé en {stopwatch.Elapsed.TotalSeconds:F2} secondes.");
        }

        var mlContext = new MLContext();
        var model = mlContext.Model.Load(modelPath, out var _);
        _predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, TriggerPrediction>(model);
    }

    /// <summary>
    /// Entraîne un modèle ML.NET et le sauvegarde au chemin spécifié.
    /// </summary>
    private void TrainModel()
    {
        var mlContext = new MLContext();

        // Charger les données
        string csvPath = "data.csv";
        if (!File.Exists(csvPath))
        {
            _logger.LogError($"Le fichier de données d'entraînement est introuvable : {csvPath}");
            throw new FileNotFoundException($"Le fichier de données d'entraînement est introuvable : {csvPath}");
        }

        var trainingDataView = mlContext.Data.LoadFromTextFile<ModelInput>(
            path: csvPath,
            hasHeader: true,
            separatorChar: ';');

        // Pipeline d'entraînement pour prédire les termes détectés
        var pipeline = mlContext.Transforms
            .CustomMapping<ModelInput, ModelOutput>(
                (input, output) =>
                {
                    output.NombreDeclencheurs = ML.IdentifierDeclencheurs(input).Count;
                    output.NiveauDeRisque = CalculateRiskLevel(input, output.NombreDeclencheurs);
                },
                contractName: "CalculateTriggersAndRisk")
            .Append(mlContext.Transforms.Conversion.ConvertType(
                outputColumnName: "NombreDeclencheursFloat",
                inputColumnName: "NombreDeclencheurs",
                outputKind: DataKind.Single))
            .Append(mlContext.Transforms.Concatenate("Features", "Age", "NombreDeclencheursFloat"))
            .Append(mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
        new Microsoft.ML.Trainers.SdcaLogisticRegressionBinaryTrainer.Options
        {
            LabelColumnName = "Label",
            FeatureColumnName = "Features",
            MaximumNumberOfIterations = 100000, // Définir le nombre maximal d'itérations
            ConvergenceTolerance = 1e-4F,      // Tolérance de convergence pour une meilleure précision
            BiasLearningRate = 0.01f          // Optionnel : ajuster le taux d'apprentissage pour le biais
        }));

        // Entraîner le modèle
        var model = pipeline.Fit(trainingDataView);

        // Sauvegarder le modèle
        mlContext.ComponentCatalog.RegisterAssembly(typeof(CalculateTriggersAndRiskMapping).Assembly);
        mlContext.Model.Save(model, trainingDataView.Schema, _modelPath);
        _logger.LogInformation($"Modèle ML.NET entraîné et sauvegardé au chemin : {_modelPath}");
    }

    private static string CalculateRiskLevel(ModelInput input, int triggerCount)
    {
        if (triggerCount == 0)
            return "None";
        if (input.Age > 30 && triggerCount >= 2 && triggerCount <= 5)
            return "Borderline";
        if (input.Age <= 30 && input.Sexe == "Homme" && triggerCount == 3)
            return "In Danger";
        if (input.Age <= 30 && input.Sexe == "Femme" && triggerCount == 4)
            return "In Danger";
        if (input.Age > 30 && triggerCount >= 6 && triggerCount <= 7)
            return "In Danger";
        if (input.Age <= 30 && input.Sexe == "Homme" && triggerCount >= 5)
            return "Early onset";
        if (input.Age <= 30 && input.Sexe == "Femme" && triggerCount >= 7)
            return "Early onset";
        if (input.Age > 30 && triggerCount >= 8)
            return "Early onset";

        return "Unknown";
    }

    public static List<string> IdentifierDeclencheurs(ModelInput input)
    {
        var declencheurs = new List<string>();

        var negations = new[]
        {
            // Négations simples
            "aucun", "sans", "ne pas", "ni", "ni de", "n'est pas", "pas de", "jamais",
            "nulle part", "aucune", "rien", "zéro", "non détecté", "ne montre pas", 

            // Négations complexes ou implicites
            "n'est plus", "ne révèle pas", "ne présente pas", "ne contient pas",
            "ne souffre pas de", "exclu", "aucune trace de", "ne signale pas",
            "ne démontre pas", "ne trouve pas", "ne permet pas de", "ne semble pas",

            // Formulations négatives indirectes
            "n'a pas été trouvé", "ne figure pas", "aucun signe de", "n'a aucun",
            "aucune indication de", "aucun élément", "aucun symptôme de",
            "ne comporte pas", "ne dispose pas de", "pas retrouvé"
        };

        // Dictionnaire des synonymes et déclencheurs principaux
        var triggerSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Hémoglobine A1C
        { "hémoglobine a1c", "Hemoglobine A1C" },
        { "hémoglobine", "Hemoglobine A1C" },
        { "taux d'hémoglobine", "Hemoglobine A1C" },
        { "glycémie", "Hemoglobine A1C" },

        // Microalbumine
        { "microalbumine", "Microalbumine" },
        { "albumine", "Microalbumine" },
        { "protéinurie", "Microalbumine" },

        // Taille
        { "taille", "Taille" },
        { "hauteur", "Taille" },
        { "grandeur", "Taille" },

        // Poids
        { "poids", "Poids" },
        { "masse", "Poids" },
        { "surpoids", "Poids" },

        // Fumeur/Fumeuse
        { "fumeur", "Fumeur" },
        { "tabac", "Fumeur" },

        // Cholestérol
        { "cholestérol", "Cholesterol" },
        { "hypercholestérolémie", "Cholesterol" },

        // Vertiges
        { "vertiges", "Vertiges" },
        { "étourdissements", "Vertiges" },

        // Rechute
        { "rechute", "Rechute" },
        { "récidive", "Rechute" },

        // Réaction
        { "réaction", "Reaction" },
        { "réaction allergique", "Reaction" },

        // Anticorps
        { "anticorps", "Anticorps" },
        { "immunoglobuline", "Anticorps" },

        // Anormal
        { "anormal", "Anormal" },
        { "anomalie", "Anormal" }
    };

        // Vérifier les colonnes structurées
        if (input.Hemoglobine_A1C == 1) declencheurs.Add("Hemoglobine A1C");
        if (input.Microalbumine == 1) declencheurs.Add("Microalbumine");
        if (input.Taille == 1) declencheurs.Add("Taille");
        if (input.Poids == 1) declencheurs.Add("Poids");
        if (input.Fumeur == 1) declencheurs.Add("Fumeur");
        if (input.Anormal == 1) declencheurs.Add("Anormal");
        if (input.Cholesterol == 1) declencheurs.Add("Cholesterol");
        if (input.Vertiges == 1) declencheurs.Add("Vertiges");
        if (input.Rechute == 1) declencheurs.Add("Rechute");
        if (input.Reaction == 1) declencheurs.Add("Reaction");
        if (input.Anticorps == 1) declencheurs.Add("Anticorps");

        // Vérifier les notes textuelles
        if (!string.IsNullOrEmpty(input.Notes))
        {
            var notesLower = input.Notes.ToLower();

            // Vérifier chaque mot-clé de déclencheur dans les notes textuelles
            foreach (var synonym in triggerSynonyms.Keys)
            {
                if (notesLower.Contains(synonym))
                {
                    // Vérifier si une négation précède ou suit le déclencheur
                    if (!IsNegated(notesLower, synonym, negations))
                    {
                        if (triggerSynonyms.TryGetValue(synonym, out var mainTrigger))
                        {
                            declencheurs.Add(mainTrigger);
                        }
                    }
                }
            }
        }

        return declencheurs.Distinct().ToList();
    }

    private static bool IsNegated(string notesLower, string trigger, string[] negations)
    {
        foreach (var negation in negations)
        {
            // Détecter les négations complexes (comme "ni de", "pas de... ni de")
            var negationPattern = $@"\b{negation}\b.*?\b{trigger}\b|\b{trigger}\b.*?\b{negation}\b";
            if (System.Text.RegularExpressions.Regex.IsMatch(notesLower, negationPattern))
            {
                return true;
            }

            // Gérer les constructions combinées comme "pas de... ni de"
            var combinedNegationPattern = $@"pas de\b.*?\bni de\b.*?\b{trigger}\b";
            if (System.Text.RegularExpressions.Regex.IsMatch(notesLower, combinedNegationPattern))
            {
                return true;
            }
        }
        return false;
    }
}

public class TriggerPrediction
{
    public bool PredictedLabel { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}

public class ModelInput
{
    [LoadColumn(0)]
    public float Age { get; set; }

    [LoadColumn(1)]
    public string Sexe { get; set; }

    [LoadColumn(2)]
    public float Hemoglobine_A1C { get; set; }

    [LoadColumn(3)]
    public float Microalbumine { get; set; }

    [LoadColumn(4)]
    public float Taille { get; set; }

    [LoadColumn(5)]
    public float Poids { get; set; }

    [LoadColumn(6)]
    public float Fumeur { get; set; }

    [LoadColumn(7)]
    public float Anormal { get; set; }

    [LoadColumn(8)]
    public float Cholesterol { get; set; }

    [LoadColumn(9)]
    public float Vertiges { get; set; }

    [LoadColumn(10)]
    public float Rechute { get; set; }

    [LoadColumn(11)]
    public float Reaction { get; set; }

    [LoadColumn(12)]
    public float Anticorps { get; set; }

    [LoadColumn(13)]
    public string Notes { get; set; }

    [LoadColumn(14)]
    public string Niveau_de_risque { get; set; }

    [LoadColumn(15)]
    [ColumnName("Label")]
    public bool Label { get; set; }
}

public class ModelOutput
{
    public string Declencheurs { get; set; }
    public int NombreDeclencheurs { get; set; }
    public string NiveauDeRisque { get; set; }
}
