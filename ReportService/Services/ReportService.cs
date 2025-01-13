using ReportService.DTOs;
using ReportService.Repositories;

namespace ReportService.Services
{
    /// <summary>
    /// Service class for managing operations on diabetes risk reports.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IMachineLearningService _machineLearningService;
        private IPatientRepository _patientRepository;
        private INoteRepository _noteRepository;

        public ReportService(IPatientRepository patientRepository,INoteRepository noteRepository, IMachineLearningService machineLearningService)
        {
            _patientRepository = patientRepository;
            _noteRepository = noteRepository;
            _machineLearningService = machineLearningService ?? throw new ArgumentNullException(nameof(machineLearningService));
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
                var patient = await _patientRepository.GetById(id);
                if (patient == null)
                {
                    return null;
                }

                var age = CalculateAge(patient.DateOfBirth);

                var notes = await _noteRepository.GetByPatientId(id);
                if (notes == null || notes.Count == 0)
                {
                    return new ReportDTO
                    {
                        PatientId = id,
                        RiskLevel = "None",
                        TriggerTerms = new List<string>()
                    };
                }

                // Initialiser une liste pour collecter les déclencheurs détectés
                var foundTriggers = new HashSet<string>();

                // Parcourir chaque note pour détecter les déclencheurs
                foreach (var note in notes)
                {
                    if (!string.IsNullOrWhiteSpace(note.Note))
                    {
                        // Utiliser TriggerAnalyzer pour détecter les déclencheurs dans la note
                        var detectedTriggers = _machineLearningService.IdentifyTriggers(new Models.MachineLearningInputModel { Notes = note.Note });
                        foreach (var trigger in detectedTriggers)
                        {
                            foundTriggers.Add(trigger);
                        }
                    }
                }

                // Compter les déclencheurs
                int triggerCount = foundTriggers.Count;

                // Appliquer les règles pour déterminer le niveau de risque
                string riskLevel = CalculateRiskLevel(age, patient.Gender, triggerCount);

                // Retourner le rapport
                return new ReportDTO
                {
                    PatientId = id,
                    RiskLevel = riskLevel,
                    TriggerTerms = foundTriggers.ToList() // Liste des notes considérées comme des déclencheurs
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
        private string CalculateRiskLevel(int age, string gender, int triggerCount)
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
                if (gender.Equals("Male", StringComparison.OrdinalIgnoreCase) && triggerCount >= 3)
                {
                    return "In Danger";
                }
                if (gender.Equals("Female", StringComparison.OrdinalIgnoreCase) && triggerCount >= 4)
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
                if (gender.Equals("Male", StringComparison.OrdinalIgnoreCase) && triggerCount >= 5)
                {
                    return "Early Onset";
                }
                if (gender.Equals("Female", StringComparison.OrdinalIgnoreCase) && triggerCount >= 7)
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

        /// <summary>
        /// Calculates age from a date of birth.
        /// </summary>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <returns>The calculated age.</returns>
        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
