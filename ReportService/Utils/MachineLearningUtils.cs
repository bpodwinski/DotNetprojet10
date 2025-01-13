using ReportService.Models;

namespace ReportService.Utils
{
    public static class MachineLearningUtils
    {
        /// <summary>
        /// Identifies triggers based on structured and textual patient data.
        /// </summary>
        /// <param name="input">The input model containing patient data.</param>
        /// <returns>A list of detected triggers.</returns>
        public static List<string> IdentifyTriggers(MachineLearningInputModel input)
        {
            var triggers = new List<string>();

            var negations = new[]
            {
                // Simple negations
                "aucun", "sans", "ne pas", "ni", "ni de", "n'est pas", "pas de", "jamais",
                "nulle part", "aucune", "rien", "zéro", "non détecté", "ne montre pas",

                // Complex or implicit negations
                "n'est plus", "ne révèle pas", "ne présente pas", "ne contient pas",
                "ne souffre pas de", "exclu", "aucune trace de", "ne signale pas",
                "ne démontre pas", "ne trouve pas", "ne permet pas de", "ne semble pas",

                // Indirect negative formulations
                "n'a pas été trouvé", "ne figure pas", "aucun signe de", "n'a aucun",
                "aucune indication de", "aucun élément", "aucun symptôme de",
                "ne comporte pas", "ne dispose pas de", "pas retrouvé"
            };

            var triggerSynonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "hémoglobine a1c", "Hemoglobine A1C" },
                { "hémoglobine", "Hemoglobine A1C" },
                { "taux d'hémoglobine", "Hemoglobine A1C" },
                { "glycémie", "Hemoglobine A1C" },
                { "microalbumine", "Microalbumine" },
                { "albumine", "Microalbumine" },
                { "protéinurie", "Microalbumine" },
                { "taille", "Taille" },
                { "hauteur", "Taille" },
                { "grandeur", "Taille" },
                { "poids", "Poids" },
                { "masse", "Poids" },
                { "surpoids", "Poids" },
                { "fumeur", "Fumeur" },
                { "tabac", "Fumeur" },
                { "cholestérol", "Cholesterol" },
                { "hypercholestérolémie", "Cholesterol" },
                { "vertiges", "Vertiges" },
                { "étourdissements", "Vertiges" },
                { "rechute", "Rechute" },
                { "récidive", "Rechute" },
                { "réaction", "Reaction" },
                { "réaction allergique", "Reaction" },
                { "anticorps", "Anticorps" },
                { "immunoglobuline", "Anticorps" },
                { "anormal", "Anormal" },
                { "anomalie", "Anormal" }
            };

            // Check structured data
            if (input.Hemoglobine_A1C == 1) triggers.Add("Hemoglobine A1C");
            if (input.Microalbumine == 1) triggers.Add("Microalbumine");
            if (input.Taille == 1) triggers.Add("Taille");
            if (input.Poids == 1) triggers.Add("Poids");
            if (input.Fumeur == 1) triggers.Add("Fumeur");
            if (input.Anormal == 1) triggers.Add("Anormal");
            if (input.Cholesterol == 1) triggers.Add("Cholesterol");
            if (input.Vertiges == 1) triggers.Add("Vertiges");
            if (input.Rechute == 1) triggers.Add("Rechute");
            if (input.Reaction == 1) triggers.Add("Reaction");
            if (input.Anticorps == 1) triggers.Add("Anticorps");

            // Check textual data
            if (!string.IsNullOrEmpty(input.Notes))
            {
                var notesLower = input.Notes.ToLower();

                foreach (var synonym in triggerSynonyms.Keys)
                {
                    if (notesLower.Contains(synonym))
                    {
                        if (!IsNegated(notesLower, synonym, negations) &&
                            triggerSynonyms.TryGetValue(synonym, out var mainTrigger))
                        {
                            triggers.Add(mainTrigger);
                        }
                    }
                }
            }

            return triggers.Distinct().ToList();
        }

        /// <summary>
        /// Calculates the risk level based on input data and the number of detected triggers.
        /// </summary>
        /// <param name="input">The input model containing patient data.</param>
        /// <param name="triggerCount">The number of detected triggers.</param>
        /// <returns>A string representing the calculated risk level.</returns>
        public static string CalculateRiskLevel(MachineLearningInputModel input, int triggerCount)
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

        /// <summary>
        /// Checks if a trigger is negated by a nearby negation phrase.
        /// </summary>
        private static bool IsNegated(string notesLower, string trigger, string[] negations)
        {
            foreach (var negation in negations)
            {
                var pattern = $@"\b{negation}\b.*?\b{trigger}\b|\b{trigger}\b.*?\b{negation}\b";
                if (System.Text.RegularExpressions.Regex.IsMatch(notesLower, pattern))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
