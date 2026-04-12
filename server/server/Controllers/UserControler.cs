using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Serilog;
using server.Models.DB;
using server.Services.DbServices.Interfaces;

namespace server.Controllers
{
    /// <summary>
    /// Controller for managing user profiles and retrieving user-related information.
    /// Access is restricted to authenticated users.
    /// </summary>
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserControler : ControllerBase
    {
        private readonly IUserService _userDbServices;

        public UserControler(IUserService userDbServices)
        {
            _userDbServices = userDbServices;
        }

        /// <summary>
        /// Retrieves a list of all registered users.
        /// Useful for administrative purposes or social features.
        /// </summary>
        /// <returns>A list of user objects.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                Log.Debug("Get allusers");
                var users = await _userDbServices.GetAllAsync();
                return Ok(users);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique GUID of the user.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userDbServices.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// Uploads and updates the profile avatar for a specific user.
        /// Supports common image formats (JPG, PNG).
        /// </summary>
        /// <param name="userId">ID of the user whose avatar is being updated.</param>
        /// <param name="avatarFile">The image file sent via multipart/form-data.</param>
        /// <response code="200">Avatar successfully uploaded and processed.</response>
        /// <response code="400">Invalid file format or upload error.</response>
        [HttpPost("{userId}/avatar/upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAvatar(Guid userId, IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var result = await _userDbServices.UploadAvatarAsync(userId, avatarFile);
            if (!result.Success)
            {
                return BadRequest(result.Data);
            }
            return Ok("Avatar uploaded successfully.");
        }
    }
}
