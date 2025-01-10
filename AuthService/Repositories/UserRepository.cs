using AuthService.Data;
using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    /// <summary>
    /// Repository class for managing User entities in the database.
    /// </summary>
    public class UserRepository(LocalDbContext dbContext, ILogger<UserRepository> logger) : IUserRepository
    {
        private readonly LocalDbContext _dbContext = dbContext;
        private readonly ILogger<UserRepository> _logger = logger;

        /// <summary>
        /// Asynchronously retrieves all User entities from the database.
        /// </summary>
        public async Task<List<UserDomain>> FindAll()
        {
            try
            {
                return await _dbContext.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously adds a new User entity to the database.
        /// </summary>
        public async Task Add(UserDomain user)
        {
            try
            {
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new user.");
                throw;
            }
        }

        /// <summary>
        /// Asynchronously finds a User entity by its ID.
        /// </summary>
        public async Task<UserDomain?> FindById(int id)
        {
            try
            {
                return await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving the user with ID {id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously finds a User entity by its username.
        /// </summary>
        public async Task<UserDomain?> FindByUserName(string username)
        {
            try
            {
                return await _dbContext.Users.FirstOrDefaultAsync(user => user.UserName == username);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while retrieving the user with username {username}.", username);
                throw;
            }
        }
    }
}
