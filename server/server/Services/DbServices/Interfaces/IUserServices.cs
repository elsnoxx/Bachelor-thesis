using server.Models.DTO;
using Microsoft.AspNetCore.Http;
using server.Models;

namespace server.Services.DbServices.Interfaces
{
    /// <summary>
    /// Service for managing user-related profile data and assets.
    /// Handles user retrieval and profile customization (e.g., avatar uploads).
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user profile by their unique ID and maps it to a Data Transfer Object.
        /// </summary>
        Task<UserDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Fetches all registered users as a collection of DTOs.
        /// </summary>
        Task<IEnumerable<UserDto>> GetAllAsync();

        /// <summary>
        /// Handles the secure storage of user profile images.
        /// Performs file type validation and updates user metadata.
        /// </summary>
        /// <returns>A result containing the relative path to the stored image.</returns>
        Task<Result<string>> UploadAvatarAsync(Guid userId, IFormFile avatarFile);
    }
}