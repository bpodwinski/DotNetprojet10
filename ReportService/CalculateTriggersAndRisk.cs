using Microsoft.ML.Transforms;

namespace ReportService
{
    [CustomMappingFactoryAttribute("CalculateTriggersAndRisk")]
    public class CalculateTriggersAndRiskMapping : CustomMappingFactory<ModelInput, ModelOutput>
    {
        public override Action<ModelInput, ModelOutput> GetMapping()
        {
            return (input, output) =>
            {
                // Identifier les déclencheurs
                var detectedTriggers = ML.IdentifierDeclencheurs(input);
                output.Declencheurs = string.Join(",", detectedTriggers);
                output.NombreDeclencheurs = detectedTriggers.Count;

                // Calculer le niveau de risque basé sur les règles
                if (output.NombreDeclencheurs == 0)
                    output.NiveauDeRisque = "None";
                else if (input.Age > 30 && output.NombreDeclencheurs >= 2 && output.NombreDeclencheurs <= 5)
                    output.NiveauDeRisque = "Borderline";
                else if (input.Age <= 30 && input.Sexe == "Homme" && output.NombreDeclencheurs == 3)
                    output.NiveauDeRisque = "In Danger";
                else if (input.Age <= 30 && input.Sexe == "Femme" && output.NombreDeclencheurs == 4)
                    output.NiveauDeRisque = "In Danger";
                else if (input.Age > 30 && output.NombreDeclencheurs >= 6 && output.NombreDeclencheurs <= 7)
                    output.NiveauDeRisque = "In Danger";
                else if (input.Age <= 30 && input.Sexe == "Homme" && output.NombreDeclencheurs >= 5)
                    output.NiveauDeRisque = "Early onset";
                else if (input.Age <= 30 && input.Sexe == "Femme" && output.NombreDeclencheurs >= 7)
                    output.NiveauDeRisque = "Early onset";
                else if (input.Age > 30 && output.NombreDeclencheurs >= 8)
                    output.NiveauDeRisque = "Early onset";
            };
        }
    }
}
