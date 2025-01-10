using Microsoft.AspNetCore.Identity;
using AuthService.Domain;
using AuthService.DTOs;
using AuthService.Repositories;
using System.Security.Claims;

namespace AuthService.Services
{
    public class UserService(IUserRepository userRepository, UserManager<UserDomain> userManager, ILogger<UserService> logger) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly UserManager<UserDomain> _userManager = userManager;
        private readonly ILogger<UserService> _logger = logger;

        /// <summary>
        /// Creates a new user based on the provided dto.
        /// </summary>
        public async Task<UserDTO?> Create(UserDTO dto)
        {
            var user = new UserDomain
            {
                UserName = dto.Username,
                FullName = dto.FullName,
                Role = dto.Role,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.Role);
                return ToOutputdto(user);
            }
            return null;
        }

        /// <summary>
        /// Deletes a user by ID. Only admins can delete any user; regular users can only delete their own account.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <param name="currentUser">The currently authenticated user performing the delete operation.</param>
        /// <returns>The deleted user's information as a <see cref="UserDTO"/>, or null if the user was not found or the deletion failed.</returns>
        public async Task<UserDTO?> DeleteById(int id, ClaimsPrincipal currentUser)
        {
            var user = await _userRepository.FindById(id);
            if (user is null)
            {
                return null;
            }

            // Check if the current user is an admin
            var isAdmin = currentUser.IsInRole("Admin");
            var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

            if (isAdmin)
            {
                _logger.LogInformation("Admin user {AdminId} is deleting user with ID {UserId}.", currentUserId, id);
            }
            else
            {
                // Check if the current user is deleting their own account
                if (currentUserId != user.Id.ToString())
                {
                    _logger.LogWarning("User {UserId} attempted to delete another user's account with ID {TargetUserId}.", currentUserId, id);
                    throw new UnauthorizedAccessException("You are only allowed to delete your own account.");
                }

                _logger.LogInformation("User {UserId} is deleting their own account.", currentUserId);
            }

            // Perform the deletion
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} successfully deleted.", id);
                return ToOutputdto(user);
            }

            _logger.LogError("Failed to delete user with ID {UserId}.", id);
            return null;
        }

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        public async Task<UserDTO?> GetById(int id)
        {
            var user = await _userRepository.FindById(id);
            return user is not null ? ToOutputdto(user) : null;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        public async Task<List<UserDTO>> GetAll()
        {
            var users = await _userRepository.FindAll();
            return users.Select(ToOutputdto).ToList();
        }

        /// <summary>
        /// Updates a user's information and resets the password if necessary.
        /// Only an admin can change roles, and password reset requires verification of the current password.
        /// </summary>
        public async Task<UserDTO?> Update(int id, UserDTO dto, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("Attempting to update user with ID {UserId}.", id);

            var user = await _userRepository.FindById(id);
            if (user is null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update.", id);
                return null;
            }

            if (dto.Role != user.Role)
            {
                if (!currentUser.IsInRole("Admin"))
                {
                    _logger.LogWarning("User with ID {CurrentUserId} attempted to change role of user {UserId} without admin rights.", currentUser.FindFirstValue(ClaimTypes.NameIdentifier), id);
                    throw new UnauthorizedAccessException("Only administrators can change user roles.");
                }

                // Empêche les utilisateurs de définir leur rôle sur "Admin" eux-mêmes
                if (dto.Role == "Admin" && currentUser.FindFirstValue(ClaimTypes.NameIdentifier) == user.Id.ToString())
                {
                    _logger.LogWarning("User with ID {UserId} attempted to set their own role to Admin.", id);
                    throw new UnauthorizedAccessException("Users cannot assign themselves the Admin role.");
                }

                // Valider que le rôle cible est correct (ex. Admin ou User uniquement)
                if (dto.Role != "Admin" && dto.Role != "User")
                {
                    _logger.LogError("Invalid role specified: {Role} for user with ID {UserId}.", dto.Role, id);
                    throw new ArgumentException("Invalid role specified.");
                }

                // Mise à jour du rôle
                _logger.LogInformation("Updating role for user with ID {UserId} to {NewRole}.", id, dto.Role);
                await _userManager.RemoveFromRoleAsync(user, user.Role);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            if (!string.IsNullOrWhiteSpace(dto.Password) && !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                // Vérifier que l'utilisateur actuel a le droit de réinitialiser le mot de passe
                var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId != user.Id.ToString() && !currentUser.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized password reset attempt by user {CurrentUserId} for user ID {UserId}.", currentUserId, id);
                    throw new UnauthorizedAccessException("Unauthorized password reset attempt.");
                }

                _logger.LogInformation("Password reset initiated for user with ID {UserId}.", id);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passwordResetResult.Succeeded)
                {
                    _logger.LogError("Password reset failed for user with ID {UserId}.", id);
                    throw new InvalidOperationException("Password reset failed.");
                }
                _logger.LogInformation("Password successfully reset for user with ID {UserId}.", id);
            }

            user.UserName = dto.Username;
            user.FullName = dto.FullName;

            _logger.LogInformation("Updating user details for user ID {UserId}.", id);
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} updated successfully.", id);
                return ToOutputdto(user);
            }
            else
            {
                _logger.LogError("Failed to update user with ID {UserId}.", id);
                return null;
            }
        }

        /// <summary>
        /// Converts a User entity to a UserDTO.
        /// </summary>
        private static UserDTO ToOutputdto(UserDomain user) => new()
        {
            Id = user.Id,
            Username = user.UserName,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}
