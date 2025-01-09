using ReportService.DTOs;

namespace ReportService.Services
{
    /// <summary>
    /// Interface for Note service to manage CRUD operations on Note entities.
    /// </summary>
    public interface IReportService
    {

        /// <summary>
        /// Asynchronously retrieves a Note entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Note to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with the NoteDTO, or null if not found.</returns>
        Task<ReportDTO?> GetDiabeteByPatientId(int id);

    }
}
