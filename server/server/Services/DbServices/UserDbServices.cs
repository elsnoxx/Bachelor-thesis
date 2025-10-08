using server.Models.DB;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;

namespace server.Services.DbServices
{
    
    public class UserDbServices : IUserDbServices
    {
        private readonly IUserRepository _userRepository;

        public UserDbServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUserAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }


    } 
}
