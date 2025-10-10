using server.Models.DB;
using server.Models.DTO;

namespace server.Services.DbServices.Interfaces
{
    public interface IUserDbServices
    {
        Task<UserDTO?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
    }
}
