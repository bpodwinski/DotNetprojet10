using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.DTOs;
using PatientService.Services;
using System.Security.Claims;

namespace PatientService.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        [HttpGet]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all users.");
                var users = await _userService.GetAll();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        [HttpPost]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(UserDTO), 201)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] UserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for adding a user.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Adding a new user.");
                var user = await _userService.Create(dto);

                if (user is not null)
                {
                    _logger.LogInformation("User created with ID {Id}.", user.Id);
                    return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
                }

                _logger.LogWarning("Unable to create user.");
                return BadRequest("Unable to create user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a user.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Fetching user with ID {Id}.", id);
                var user = await _userService.GetById(id);
                if (user is not null)
                {
                    return Ok(user);
                }
                _logger.LogWarning("User with ID {Id} not found.", id);
                return NotFound($"User with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Updates a user's details by ID.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="dto">The updated user data.</param>
        /// <returns>A response indicating success or failure of the update.</returns>
        [HttpPut("{id}")]
        [Authorize(policy: "User")]
        [ProducesResponseType(typeof(UserDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] UserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating user with ID {Id}.", id);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Updating user with ID {Id}.", id);

                var currentUser = User;
                var user = await _userService.Update(id, dto, currentUser);

                if (user is not null)
                {
                    _logger.LogInformation("User with ID {Id} updated successfully.", id);
                    return Ok(user);
                }

                // Si l'utilisateur n'a pas été trouvé
                _logger.LogWarning("User with ID {Id} not found for update.", id);
                return NotFound($"User with ID {id} not found.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to update user with ID {Id}.", id);
                return StatusCode(403, "You do not have permission to perform this action.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument provided for updating user with ID {Id}.", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An internal error occurred while updating user with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Deletes a user by ID. Admins can delete any user, but regular users can only delete their own account.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "User")]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var isAdmin = User.IsInRole("Admin");

                // Check if the user is allowed to delete the target user
                if (!isAdmin && id != currentUserId)
                {
                    _logger.LogWarning("User with ID {UserId} attempted to delete another user's account with ID {TargetId}.", currentUserId, id);
                    return StatusCode(403, new { message = "You are not authorized to delete other users' accounts." });
                }

                if (id == currentUserId && !isAdmin)
                {
                    _logger.LogInformation("User with ID {UserId} is deleting their own account.", currentUserId);
                }
                else
                {
                    _logger.LogInformation("Admin user with ID {AdminId} is deleting user with ID {UserId}.", currentUserId, id);
                }

                // Attempt to delete the user
                var user = await _userService.DeleteById(id, User);
                if (user is not null)
                {
                    return NoContent();
                }

                _logger.LogWarning("User with ID {Id} not found for deletion.", id);
                return NotFound(new { message = $"User with ID {id} not found." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized deletion attempt by user ID {UserId}.", id);
                return StatusCode(403, new { message = "Unauthorized deletion attempt by user ID" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
