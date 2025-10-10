using AutoMapper;
using Org.BouncyCastle.Crypto;
using server.Models.DB;
using server.Models.DTO;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;

namespace server.Services.DbServices
{
    
    public class UserDbServices : IUserDbServices
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;


        public UserDbServices(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUserAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDTO>>(users);
            return userDtos;
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid id)
        {
            var user =  await _userRepository.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserDTO>(user);

        }


    } 
}
