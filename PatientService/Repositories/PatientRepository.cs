using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Domain;

namespace PatientService.Repositories
{
    /// <summary>
    /// Repository class for managing Patient entities in the database.
    /// </summary>
    public class PatientRepository(LocalDbContext dbContext, ILogger<PatientRepository> logger) : IPatientRepository
    {
        private readonly LocalDbContext _dbContext = dbContext;
        private readonly ILogger<PatientRepository> _logger = logger;

        /// <summary>
        /// Asynchronously creates a new Patient entity and saves it to the database.
        /// </summary>
        /// <param name="patient">The Patient entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Create(PatientDomain patient)
        {
            try
            {
                await _dbContext.Patients.AddAsync(patient);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully created Patient with ID {PatientId}.", patient.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new Patient.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously deletes a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient entity to delete.</param>
        /// <returns>The deleted Patient entity, or null if not found.</returns>
        public async Task<PatientDomain?> DeleteById(int id)
        {
            try
            {
                var patient = await _dbContext.Patients.FindAsync(id);

                if (patient == null)
                {
                    _logger.LogWarning("Attempted to delete Patient with ID {Id}, but it was not found.", id);
                    return null;
                }

                _dbContext.Patients.Remove(patient);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted Patient with ID {Id}.", id);

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Patient with ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient entity to retrieve.</param>
        /// <returns>The Patient entity, or null if not found.</returns>
        public async Task<PatientDomain?> GetById(int id)
        {
            try
            {
                var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id);
                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {Id} was not found.", id);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved Patient with ID {Id}.", id);
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving Patient with ID {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all Patient entities from the database.
        /// </summary>
        /// <returns>An IQueryable of Patient entities.</returns>
        public IQueryable<PatientDomain> GetAll()
        {
            try
            {
                _logger.LogInformation("Retrieving all Patient entities.");
                return _dbContext.Patients.AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all Patient entities.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously updates an existing Patient entity and saves changes to the database.
        /// </summary>
        /// <param name="patient">The Patient entity with updated values.</param>
        /// <returns>The updated Patient entity, or null if not found.</returns>
        public async Task<PatientDomain?> Update(PatientDomain patient)
        {
            try
            {
                _logger.LogInformation("Attempting to update Patient with ID {PatientId}.", patient.Id);

                var existingPatient = await _dbContext.Patients.FindAsync(patient.Id);

                if (existingPatient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found for update.", patient.Id);
                    return null;
                }

                _dbContext.Entry(existingPatient).CurrentValues.SetValues(patient);

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully updated Patient with ID {PatientId}.", patient.Id);

                return existingPatient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the Patient with ID {PatientId}.", patient.Id);
                throw;
            }
        }
    }
}
