using server.Models.DB;

namespace server.Services.DbServices.Interfaces
{
    public interface IUserDbServices
    {
        Task<User?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
