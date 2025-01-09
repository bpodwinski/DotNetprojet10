using AuthService.Domain;

namespace AuthService.Repositories
{
    /// <summary>
    /// Interface for managing User entities in the database.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Asynchronously retrieves all User entities from the database.
        /// </summary>
        /// <returns>A list of User entities.</returns>
        Task<List<UserDomain>> FindAll();

        /// <summary>
        /// Asynchronously adds a new User entity to the database.
        /// </summary>
        /// <param name="user">The User entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Add(UserDomain user);

        /// <summary>
        /// Asynchronously retrieves a User entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the User entity to retrieve.</param>
        /// <returns>The User entity, or null if not found.</returns>
        Task<UserDomain?> FindById(int id);

        /// <summary>
        /// Asynchronously retrieves a User entity by its username.
        /// </summary>
        /// <param name="username">The username of the User entity to retrieve.</param>
        /// <returns>The User entity, or null if not found.</returns>
        Task<UserDomain?> FindByUserName(string username);
    }
}
