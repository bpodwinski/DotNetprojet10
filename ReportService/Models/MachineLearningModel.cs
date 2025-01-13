using Microsoft.ML.Data;

namespace ReportService.Models
{
    /// <summary>
    /// Represents the input data schema for the ML model
    /// </summary>
    public class MachineLearningInputModel
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

    /// <summary>
    /// Represents the output model for machine learning operations.
    /// </summary>
    public class MachineLearningOutputModel
    {
        /// <summary>
        /// Gets or sets the list of triggers identified during processing, represented as a comma-separated string.
        /// </summary>
        public string Triggers { get; set; }

        /// <summary>
        /// Gets or sets the total number of triggers identified during processing.
        /// </summary>
        public int TriggerCount { get; set; }

        /// <summary>
        /// Gets or sets the calculated risk level based on the identified triggers and input data.
        /// </summary>
        public string RiskLevel { get; set; }
    }
}
