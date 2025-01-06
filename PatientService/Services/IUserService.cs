using PatientService.DTOs;
using System.Security.Claims;

namespace PatientService.Services
{
    /// <summary>
    /// Interface for managing operations on User entities.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Asynchronously creates a new User.
        /// </summary>
        /// <param name="dto">The UserDTO containing the details of the user to create.</param>
        /// <returns>The created UserDTO, or null if the creation failed.</returns>
        Task<UserDTO?> Create(UserDTO dto);

        /// <summary>
        /// Asynchronously deletes a User by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <param name="currentUser">The currently authenticated user performing delete operation.</param>
        /// <returns>The deleted UserDTO, or null if the user was not found.</returns>
        Task<UserDTO?> DeleteById(int id, ClaimsPrincipal currentUser);

        /// <summary>
        /// Asynchronously retrieves a User by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The UserDTO, or null if the user was not found.</returns>
        Task<UserDTO?> GetById(int id);

        /// <summary>
        /// Asynchronously retrieves all User entities.
        /// </summary>
        /// <returns>A list of UserDTOs.</returns>
        Task<List<UserDTO>> GetAll();

        /// <summary>
        /// Asynchronously updates an existing User by its ID.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="dto">The UserDTO containing the updated details of the user.</param>
        /// <param name="currentUser">The currently authenticated user performing update operation.</param>
        /// <returns>The updated UserDTO, or null if the user was not found.</returns>
        Task<UserDTO?> Update(int id, UserDTO dto, ClaimsPrincipal currentUser);
    }
}
