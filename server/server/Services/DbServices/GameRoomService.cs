using AutoMapper;
using Microsoft.VisualBasic;
using Serilog;
using server.Models;
using server.Models.DB;
using server.Models.DTO;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;
using System.Security.AccessControl;

namespace server.Services.DbServices
{
    public class GameRoomService : IGameRoomService
    {
        private readonly IGameRoomRepository _gameRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISesionRepository _sesionRepository;
        private readonly IMapper _mapper;

        public GameRoomService(IGameRoomRepository gameRoomRepository, IUserRepository userRepository, ISesionRepository sesionRepository, IMapper mapper)
        {
            _gameRoomRepository = gameRoomRepository;
            _userRepository = userRepository;
            _sesionRepository = sesionRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<GameRoomDTO>>> GetGameRoomsListAsync(string gameType)
        {
            var gameRooms = await _gameRoomRepository.AllGameRoomsAsync(gameType);
            var gameRoomsDTO = _mapper.Map<IEnumerable<GameRoomDTO>>(gameRooms);

            for(var i = 0; i < gameRooms.Count(); i++)
            {
                if (gameRooms.ElementAt(i).PasswordHash != null)
                {
                    gameRoomsDTO.ElementAt(i).password = "***";
                }

                var usersInRoom = await _sesionRepository.GetUsersInGameRoomAsync(gameRooms.ElementAt(i).Id);
                gameRoomsDTO.ElementAt(i).CurrentPlayers = usersInRoom.Count();
            }

            return Result<IEnumerable<GameRoomDTO>>.Ok(gameRoomsDTO);
        }

        public async Task<Result<IEnumerable<UserDTO>>> GetUsersGameRoomAsync(Guid gameRoomId)
        {
            var checkRoom = await _gameRoomRepository.GameRoomExistsAsync(gameRoomId);
            if (checkRoom)
            {
                Log.Error("Game Room {RoomId} not found.", gameRoomId);
                return Result<IEnumerable<UserDTO>>.Fail("Game Room not found.");
            }

            var users = _sesionRepository.GetUsersInGameRoomAsync(gameRoomId);
            var usersDTO = _mapper.Map<IEnumerable<UserDTO>>(users);
            return Result<IEnumerable<UserDTO>>.Ok(usersDTO);
        }

        public async Task<Result<bool>> CreateGameRoomAsync(GameRoomCreationDTO gameRoomDTO)
        {
            var user = await _userRepository.GetByEmailAsync(gameRoomDTO.UserId);
            var gameRoom = CreateGameRoom(gameRoomDTO, user.Id);
            try
            {
                await _gameRoomRepository.CreateGameRoomAsync(gameRoom);
                Log.Information("Game room {RoomId} created successfully", gameRoom.Id);
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating game room");
                return Result<bool>.Fail("Failed to create game room.");
            }

        }

        public async Task<Result<bool>> JoinGameRoomAsync(Guid gameRoomId, JoinRoomRequest request)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.UserEmail);
                var gameRoom = await _gameRoomRepository.GameRoomById(gameRoomId);
                if (user == null || gameRoom == null)
                {
                    Log.Error("User or Game Room not found.");
                    return Result<bool>.Fail("User or Game Room not found.");
                }

                var usersInRoom = await _sesionRepository.GetUsersInGameRoomAsync(gameRoomId);
                if(usersInRoom.Count() >= gameRoom.MaxPlayers)
                {
                    Log.Error("Game Room is full.");    
                    return Result<bool>.Fail("Game Room is full.");
                }
                else if ( usersInRoom.Contains(user.Id) == true )
                {
                    Log.Error("User already in the Game Room.");
                    return Result<bool>.Fail("User already in the Game Room.");
                }

                if (gameRoom.PasswordHash != null)
                {
                    if (string.IsNullOrWhiteSpace(request.Password) || !BCrypt.Net.BCrypt.Verify(request.Password, gameRoom.PasswordHash))
                    {
                        Log.Error("Invalid password");
                        return Result<bool>.Fail("Invalid password.");
                    }
                    else
                    {
                        Log.Debug("User {UserId} provided correct password for room {RoomId}", user.Id, gameRoomId);
                    }
                }

                var session = CreateSesion(user.Id, gameRoomId);

                var added = await _sesionRepository.AddUserToSesion(session);
                if (added)
                {
                    Log.Debug("User {UserId} joined game room {RoomId}", user.Id, gameRoomId);
                    return Result<bool>.Ok(true);
                }
                Log.Error("Failed to add user to session");
                return Result<bool>.Fail("Failed to join game room.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating game room");
                return Result<bool>.Fail("Failed to create game room.");
            }
        }

        public async Task<Result<bool>> LeaveGameRoomAsync(Guid gameRoomId, LeaveRoomRequest request)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.userEmail);
                var gameRoom = await _gameRoomRepository.GameRoomById(gameRoomId);
                if (user == null || gameRoom == null)
                {
                    Log.Error("User or Game Room not found.");
                    return Result<bool>.Fail("User or Game Room not found.");
                }
                var usersInRoom = await _sesionRepository.GetUsersInGameRoomAsync(gameRoomId);
                if (usersInRoom.Contains(user.Id) == false)
                {
                    Log.Error("User not in the Game Room.");
                    return Result<bool>.Fail("User not in the Game Room.");
                }
                var removed = await _sesionRepository.RemoveUserFromSesion(user.Id, gameRoomId);
                if (removed)
                {
                    Log.Debug("User {UserId} left game room {RoomId}", user.Id, gameRoomId);
                    return Result<bool>.Ok(true);
                }
                Log.Error("Failed to remove user from session");
                return Result<bool>.Fail("Failed to leave game room.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error leaving game room");
                return Result<bool>.Fail("Failed to leave game room.");
            }
        }


        private GameRoom CreateGameRoom(GameRoomCreationDTO gameRoomDTO, Guid userId)
        {
            var password = string.IsNullOrWhiteSpace(gameRoomDTO.password) ? null : gameRoomDTO.password;

            var gameRoom = new GameRoom
            {
                Id = Guid.NewGuid(),
                Name = gameRoomDTO.Name,
                GameType = gameRoomDTO.GameType,
                MaxPlayers = gameRoomDTO.MaxPlayers,
                CreatedBy = userId,
                PasswordHash = password != null ? BCrypt.Net.BCrypt.HashPassword(password) : null,
                Status = "Waiting",
                CreatedAt = DateTime.UtcNow
            };

            return gameRoom;
        }

        private Session CreateSesion(Guid userId, Guid gameRoomId)
        {
            return new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameRoomId = gameRoomId,
                JoinedAt = DateTime.UtcNow
            };
        }
    }
}
