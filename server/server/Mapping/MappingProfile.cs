using AutoMapper;
using server.Models.DB;
using server.Models.DTO;

namespace server.Mapping
{
    /// <summary>
    /// Configures object-to-object mapping rules using AutoMapper.
    /// This profile defines how database entities are transformed into Data Transfer Objects (DTOs),
    /// ensuring that internal database structures are not directly exposed to the API clients.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map User entity to UserDTO for public profile viewing or identification
            CreateMap<User, UserDto>();

            // Map GameRoom entity to GameRoomDTO to provide a simplified view of active games
            // This prevents exposing internal properties like Creator reference or Session lists if not needed
            CreateMap<GameRoom, GameRoomDTO>();
        }
    }
}