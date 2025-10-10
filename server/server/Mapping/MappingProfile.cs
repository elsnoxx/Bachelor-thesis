using AutoMapper;
using server.Models.DB;
using server.Models.DTO;

namespace server.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<GameRoom, GameRoomDTO>();
        }
    }
}