using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Domain;
using PatientService.DTOs;
using PatientService.Repositories;

namespace PatientService.Services
{
    /// <summary>
    /// Service class for managing operations on Patient entities.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly LocalDbContext _dbContext;

        public PatientService(
            IPatientRepository patientRepository,
            LocalDbContext dbContext
        )
        {
            _patientRepository = patientRepository;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Asynchronously creates a new Patient entity based on the provided DTO.
        /// </summary>
        /// <param name="dto">The DTO containing data for the new Patient.</param>
        /// <returns>The created PatientDTO.</returns>
        public async Task<PatientDTO?> Create(PatientDTO dto)
        {
            try
            {
                var patient = ToPatient(dto);

                await _patientRepository.Create(patient);
                return ToPatientDTO(patient);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the Patient.", ex);
            }
        }

        /// <summary>
        /// Asynchronously deletes a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient to delete.</param>
        /// <returns>The deleted PatientDTO, or null if not found.</returns>
        public async Task<PatientDTO?> DeleteById(int id)
        {
            try
            {
                var existingPatient = await _patientRepository.GetById(id);
                if (existingPatient == null)
                {
                    return null;
                }

                _dbContext.Entry(existingPatient).State = EntityState.Detached;

                var deletedPatient = await _patientRepository.DeleteById(id);
                return deletedPatient is not null ? ToPatientDTO(deletedPatient) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the Patient with ID {id}.", ex);
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Patient entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the Patient to retrieve.</param>
        /// <returns>The PatientDTO, or null if not found.</returns>
        public async Task<PatientDTO?> GetById(int id)
        {
            try
            {
                var patient = await _patientRepository.GetById(id);
                return patient is not null ? ToPatientDTO(patient) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the Patient with ID {id}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all Patient entities and converts them to PatientDTOs.
        /// </summary>
        /// <returns>A list of PatientDTOs.</returns>
        public async Task<List<PatientDTO>> GetAll()
        {
            try
            {
                var patients = await _patientRepository.GetAll().ToListAsync();
                return patients.Select(ToPatientDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving all Patient entities.", ex);
            }
        }

        /// <summary>
        /// Asynchronously updates a Patient entity with the provided data.
        /// </summary>
        /// <param name="id">The ID of the Patient to update.</param>
        /// <param name="dto">The DTO containing the updated values.</param>
        /// <returns>The updated PatientDTO, or null if not found.</returns>
        /// <exception cref="Exception">Throws an exception with a detailed message if the Patient is not found or if an error occurs during the update.</exception>
        public async Task<PatientDTO?> Update(int id, PatientDTO dto)
        {
            try
            {
                var existingPatient = await _patientRepository.GetById(id) ?? throw new Exception($"Patient with ID {id} not found.");
                var patient = ToPatient(dto);
                patient.Id = id;

                var updatedPatient = await _patientRepository.Update(patient);
                return updatedPatient != null ? ToPatientDTO(updatedPatient) : null;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the Patient entity.", ex);
            }
        }

        /// <summary>
        /// Converts a PatientDTO to a PatientDomain entity.
        /// </summary>
        /// <param name="dto">The PatientDTO containing data.</param>
        /// <returns>The corresponding PatientDomain entity.</returns>
        private static PatientDomain ToPatient(PatientDTO dto) => new()
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            PhoneNumber = dto.PhoneNumber
        };

        /// <summary>
        /// Converts a PatientDomain entity to a PatientDTO.
        /// </summary>
        /// <param name="patientDomain">The PatientDomain entity to convert.</param>
        /// <returns>The corresponding PatientDTO.</returns>
        private static PatientDTO ToPatientDTO(PatientDomain patientDomain) => new()
        {
            Id = patientDomain.Id,
            FirstName = patientDomain.FirstName,
            LastName = patientDomain.LastName,
            DateOfBirth = patientDomain.DateOfBirth,
            Gender = patientDomain.Gender,
            Address = patientDomain.Address,
            PhoneNumber = patientDomain.PhoneNumber
        };
    }
}
