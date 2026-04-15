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
    /// <summary>
    /// Implementation of user management services.
    /// Bridges the gap between raw data repositories and API controllers.
    /// </summary>
    public class UserServices : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly FileHelper _fileHelper;

        public UserServices(IUserRepository userRepository, IMapper mapper, FileHelper fileHelper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _fileHelper = fileHelper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return userDtos;
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user =  await _userRepository.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserDto>(user);

        }

        /// <summary>
        /// Securely uploads a user avatar.
        /// Includes validation for file existence and MIME type to prevent malicious uploads.
        /// </summary>
        public async Task<Result<string>> UploadAvatarAsync(Guid userId, IFormFile avatarFile)
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
                var relativePath = await _fileHelper.SaveFileAsync("avatars", avatarFile);

                user.AvatarUrl = relativePath;
                await _userRepository.UpdateAsync(user);

                return Result<string>.Ok(relativePath);
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Error uploading avatar: {ex.Message}");
            }
        }

    }
}
