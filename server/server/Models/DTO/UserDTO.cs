namespace server.Models.DTO
{
    /// <summary>
    /// Safe representation of a user profile for public display.
    /// Excludes sensitive data like PasswordHash or RefreshTokens.
    /// </summary>
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }

        /// <summary>
        /// Full URL to the user's avatar image.
        /// </summary>
        public string AvatarUrl { get; set; }
    }
}