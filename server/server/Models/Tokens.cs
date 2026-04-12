namespace server.Models.DTO
{
    /// <summary>
    /// Holds a pair of tokens required for secure user authentication and session renewal.
    /// </summary>
    public class Tokens
    {
        /// <summary>
        /// Short-lived JSON Web Token (JWT) for authorizing API requests.
        /// </summary>
        public string TokenJWT { get; set; } = string.Empty;

        /// <summary>
        /// Long-lived token used to obtain a new JWT without re-authenticating.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}