using ReportService.DTOs;

namespace ReportService.Services
{
    /// <summary>
    /// Service class for managing operations on diabetes risk reports.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IPatientRepository _patientRepositor;

        public ReportService(IPatientRepositore patientRepositor)
        {
            _patientRepositor = patientRepositor;
        }

        /// <summary>
        /// Asynchronously retrieves a diabetes risk report for a patient by their ID.
        /// </summary>
        /// <param name="id">The ID of the patient.</param>
        /// <returns>The ReportDTO, or null if not found.</returns>
        public async Task<ReportDTO?> GetDiabeteByPatientId(int id)
        {
            try
            {
                // Récupérer les informations du patient
                var patient = await _patientService.GetPatientById(id);
                if (patient == null)
                {
                    return null; // Patient non trouvé
                }

                // Récupérer les notes médicales du patient
                var notes = await _noteService.GetNotesByPatientId(id);
                if (notes == null || !notes.Any())
                {
                    return new ReportDTO
                    {
                        PatientId = id,
                        RiskLevel = "None",
                        TriggerTerms = new List<string>()
                    };
                }

                // Liste des termes déclencheurs
                var triggerTerms = new List<string>
                {
                    "Hémoglobine A1C", "Microalbumine", "Taille", "Poids",
                    "Fumeur", "Fumeuse", "Anormal", "Cholestérol",
                    "Vertiges", "Rechute", "Réaction", "Anticorps"
                };

                // Recherche des termes déclencheurs dans les notes médicales
                var foundTriggers = notes
                    .SelectMany(note => triggerTerms.Where(term =>
                        note.Content.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .Distinct()
                    .ToList();

                // Compter les déclencheurs
                int triggerCount = foundTriggers.Count;

                // Appliquer les règles pour déterminer le niveau de risque
                string riskLevel = CalculateRiskLevel(patient.Age, patient.Sex, triggerCount);

                // Retourner le rapport
                return new ReportDTO
                {
                    PatientId = id,
                    RiskLevel = riskLevel,
                    TriggerTerms = foundTriggers
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the diabetes risk report for patient with ID {id}.", ex);
            }
        }

        /// <summary>
        /// Determines the diabetes risk level based on age, sex, and trigger count.
        /// </summary>
        private string CalculateRiskLevel(int age, string sex, int triggerCount)
        {
            if (triggerCount == 0)
            {
                return "None";
            }

            if (triggerCount >= 2 && triggerCount <= 5 && age > 30)
            {
                return "Borderline";
            }

            if (age < 30)
            {
                if (sex.Equals("Male", StringComparison.OrdinalIgnoreCase) && triggerCount >= 3)
                {
                    return "In Danger";
                }
                if (sex.Equals("Female", StringComparison.OrdinalIgnoreCase) && triggerCount >= 4)
                {
                    return "In Danger";
                }
            }
            else if (age > 30 && (triggerCount == 6 || triggerCount == 7))
            {
                return "In Danger";
            }

            if (age < 30)
            {
                if (sex.Equals("Male", StringComparison.OrdinalIgnoreCase) && triggerCount >= 5)
                {
                    return "Early Onset";
                }
                if (sex.Equals("Female", StringComparison.OrdinalIgnoreCase) && triggerCount >= 7)
                {
                    return "Early Onset";
                }
            }
            else if (age > 30 && triggerCount >= 8)
            {
                return "Early Onset";
            }

            return "None"; // Cas par défaut
        }
    }
}
