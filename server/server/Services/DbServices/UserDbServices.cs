using AutoMapper;
using Org.BouncyCastle.Crypto;
using server.Helpers;
using server.Models;
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
        private readonly IWebHostEnvironment _env;
        private readonly FileHelper _fileHelper;

        public UserDbServices(IUserRepository userRepository, IMapper mapper, FileHelper fileHelper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _fileHelper = fileHelper;
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

        public async Task<Result<string>> UploadUserAvatarAsync(Guid userId, IFormFile avatarFile)
        {
            var allowedTypes = new[] { "image/png", "image/jpeg" };

            // 1️⃣ Ověř uživatele
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<string>.Fail("User not found.");

            // 2️⃣ Validace typu souboru
            if (avatarFile == null || avatarFile.Length == 0)
                return Result<string>.Fail("Invalid file.");

            if (!allowedTypes.Contains(avatarFile.ContentType))
                return Result<string>.Fail("Only PNG and JPEG formats are allowed.");

            try
            {
                // 3️⃣ Uložení souboru přes FileHelper (včetně vytvoření složky, názvu, kopírování)
                var relativePath = await _fileHelper.SaveFileAsync("avatars", avatarFile);

                // 4️⃣ Aktualizace uživatele v DB
                user.AvatarUrl = relativePath;
                await _userRepository.UpdateUserAsync(user);

                return Result<string>.Ok(relativePath);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Error uploading avatar: {ex.Message}");
            }
        }

    }
}
