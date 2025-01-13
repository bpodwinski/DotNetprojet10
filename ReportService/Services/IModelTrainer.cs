namespace ReportService.Services
{
    /// <summary>
    /// Defines the contract for training a machine learning model.
    /// </summary>
    public interface IModelTrainer
    {
        /// <summary>
        /// Trains a machine learning model using data from the specified CSV file.
        /// </summary>
        /// <param name="csvPath">The path to the CSV file containing training data.</param>
        /// <exception cref="FileNotFoundException">Thrown when the specified file cannot be found.</exception>
        /// <exception cref="InvalidOperationException">Thrown when training fails due to invalid data or configuration.</exception>
        void TrainModel(string csvPath);
    }
}
