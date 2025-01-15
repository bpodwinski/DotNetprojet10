using Humanizer;
using ReportService.DTOs;
using ReportService.Models;
using ReportService.Repositories;

namespace ReportService.Services
{
    /// <summary>
    /// Service class for managing operations on diabetes risk reports.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IElasticsearchService _elasticSearchService;
        //private readonly IMachineLearningService _machineLearningService;
        private IPatientRepository _patientRepository;
        private INoteRepository _noteRepository;

        public ReportService(
            IPatientRepository patientRepository,
            INoteRepository noteRepository,
            //IMachineLearningService machineLearningService,
            IElasticsearchService elasticSearchService
        )
        {
            _patientRepository = patientRepository;
            _noteRepository = noteRepository;
            //_machineLearningService = machineLearningService ?? throw new ArgumentNullException(nameof(machineLearningService));
            _elasticSearchService = elasticSearchService;
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
                        TriggerTerms = []
                    };
                }

                // Clear existing patient notes in Elasticsearch for consistency
                await _elasticSearchService.DeleteNotesByPatientIdAsync("medical_notes", id);

                // Index new patient notes in Elasticsearch
                foreach (var note in notes)
                {
                    if (!string.IsNullOrWhiteSpace(note.Note))
                    {
                        var document = new MedicalNoteModel
                        {
                            NoteId = note.Id,
                            PatientId = id,
                            Note = note.Note,
                            Date = note.Date
                        };

                        await _elasticSearchService.IndexDocumentAsync("medical_notes", document);
                    }
                }

                var triggerTerms = new List<TriggerTerm>
                {
                    new("Hémoglobine A1C", "Biologique", ["HbA1C", "Hémoglobine glyquée", "Hémoglobyne A1C", "Hémoglobyne glikée"]),
                    new("Microalbumine", "Biologique", ["Albumine urinaire", "Protéines urinaires", "Mikroalbumine", "Micralbumine"]),
                    new("Taille", "Physique", ["Hauteur", "Stature", "Tayle", "Tail"]),
                    new("Poids", "Physique", ["Masse corporelle", "Poid", "Poyds"]),
                    new("Surpoids", "Physique", ["Sur poids", "Excès de poids", "Obésité", "Surpoid", "Surtpoids"]),
                    new("Fumeur", "Habitude", ["Tabagisme", "Consommation de tabac", "Fumeure", "Fumer"]),
                    new("Fumeuse", "Habitude", ["Tabagisme féminin", "Consommatrice de tabac", "Fumeuze", "Fumeusses"]),
                    new("Anormal", "État", ["Irrégulier", "Pathologique", "Anormalle", "Anormale"]),
                    new("Cholestérol", "Biologique", ["LDL", "HDL", "Triglycérides", "Cholesterole", "Colestérol"]),
                    new("Vertiges", "Symptôme", ["Étourdissements", "Tête qui tourne", "Vertige", "Verstiges"]),
                    new("Rechute", "Symptôme", ["Récidive", "Retour des symptômes", "Réchute", "Rechutte"]),
                    new("Réaction", "Symptôme", ["Réaction allergique", "Effet indésirable", "Réactionne", "Réaxion"]),
                    new("Anticorps", "Biologique", ["Immunoglobulines", "Réponse immunitaire", "Antycorps", "Antikorps"])
                };

                var terms = triggerTerms.Select(t => t.Term).ToList();
                var allNotes = string.Join(" ", notes.Select(n => n.Note));

                var detectedTriggers = await _elasticSearchService.SearchAsync(
                    indexName: "medical_notes",
                    terms: terms,
                    patientId: id
                );

                var foundTriggers = detectedTriggers
                    .SelectMany(hit => hit.HighlightedTriggers)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                //foreach (var note in notes)
                //{
                //    if (!string.IsNullOrWhiteSpace(note.Note))
                //    {
                //        // Utiliser TriggerAnalyzer pour détecter les déclencheurs dans la note
                //        var detectedTriggers = _machineLearningService.IdentifyTriggers(new Models.MachineLearningInputModel { Notes = note.Note });
                //        foreach (var trigger in detectedTriggers)
                //        {
                //            foundTriggers.Add(trigger);
                //        }
                //    }
                //}

                int triggerCount = foundTriggers.Count;

                string riskLevel = CalculateRiskLevel(age, patient.Gender, triggerCount);

                return new ReportDTO
                {
                    PatientId = id,
                    RiskLevel = riskLevel,
                    TriggerTerms = [.. foundTriggers]
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
        /// <summary>
        /// Determines the diabetes risk level based on age, sex, and trigger count.
        /// </summary>
        private static string CalculateRiskLevel(int age, string gender, int triggerCount)
        {
            if (triggerCount == 0)
            {
                return "None";
            }

            if (age > 30)
            {
                if (triggerCount >= 2 && triggerCount <= 5)
                {
                    return "Borderline";
                }
                if (triggerCount == 6 || triggerCount == 7)
                {
                    return "In Danger";
                }
                if (triggerCount >= 8)
                {
                    return "Early Onset";
                }
            }
            else // age <= 30
            {
                if (gender.Equals("Male", StringComparison.OrdinalIgnoreCase))
                {
                    if (triggerCount >= 5)
                    {
                        return "Early Onset";
                    }
                    if (triggerCount >= 3)
                    {
                        return "In Danger";
                    }
                }
                else if (gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
                {
                    if (triggerCount >= 7)
                    {
                        return "Early Onset";
                    }
                    if (triggerCount >= 4)
                    {
                        return "In Danger";
                    }
                }
            }

            return "None";
        }

        /// <summary>
        /// Calculates age from a date of birth.
        /// </summary>
        /// <param name="dateOfBirth">The date of birth.</param>
        /// <returns>The calculated age.</returns>
        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
