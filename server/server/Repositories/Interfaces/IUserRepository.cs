using server.Models.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace server.Repositories.Interfaces
{
    /// <summary>
    /// Interface for user data persistence. 
    /// Handles user identity management, registration, and profile updates.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their unique database identifier.
        /// </summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>
        /// Fetches all registered users. Useful for administrative purposes.
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Finds a user by their unique username.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Finds a user by their registered email address.
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Persists a new user entity to the database.
        /// </summary>
        Task AddAsync(User user);

        /// <summary>
        /// Synchronizes changes made to a user entity with the database.
        /// </summary>
        /// <returns>True if the user was found and updated; otherwise, false.</returns>
        Task<bool> UpdateAsync(User user);

        /// <summary>
        /// Manually triggers saving of all pending changes in the context.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Updates the 'LastLogin' timestamp for a specific user.
        /// </summary>
        Task UpdateLastLoginAsync(User user);

        /// <summary>
        /// Locates a user based on a unique token sent for email verification.
        /// </summary>
        Task<User?> GetByEmailConfirmationTokenAsync(Guid token);
    }
}