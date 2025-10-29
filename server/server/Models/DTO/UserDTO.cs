using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace server.Models.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
    }
}
