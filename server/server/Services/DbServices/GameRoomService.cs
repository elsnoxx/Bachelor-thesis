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
        private readonly ISessionRepository _sessionRepository;
        private readonly IMapper _mapper;

        public GameRoomService(IGameRoomRepository gameRoomRepository, IUserRepository userRepository, ISessionRepository sessionRepository, IMapper mapper)
        {
            _gameRoomRepository = gameRoomRepository;
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _mapper = mapper;
        }

        public async Task FinishGameRoomAsync(Guid gameroomid)
        {
            var gameRoom = await _gameRoomRepository.GetByIdAsync(gameroomid);
            if (gameRoom == null)
            {
                Log.Error("Game Room {RoomId} not found.", gameroomid);
                return;
            }
            gameRoom.Status = "Finished";
            try
            {
                await _gameRoomRepository.UpdateAsync(gameRoom);
                Log.Information("Game room {RoomId} marked as finished", gameroomid);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating game room status");
            }
        }

        public async Task StartGameRoomAsync(Guid gameroomid)
        {
            var gameRoom = await _gameRoomRepository.GetByIdAsync(gameroomid);
            if (gameRoom == null)
            {
                Log.Error("Game Room {RoomId} not found.", gameroomid);
                return;
            }
            gameRoom.Status = "InProgress";
            try
            {
                await _gameRoomRepository.UpdateAsync(gameRoom);
                Log.Information("Game room {RoomId} marked as in progress", gameroomid);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating game room status");
            }
        }

        public async Task<Result<IEnumerable<GameRoomDTO>>> GetGameRoomsListAsync(string gameType)
        {
            var gameRooms = await _gameRoomRepository.GetAvailableRoomsAsync(gameType);
            var gameRoomsDTO = _mapper.Map<IEnumerable<GameRoomDTO>>(gameRooms);

            for(var i = 0; i < gameRooms.Count(); i++)
            {
                if (gameRooms.ElementAt(i).PasswordHash != null)
                {
                    gameRoomsDTO.ElementAt(i).Password = "***";
                }

                var usersInRoom = await _sessionRepository.GetUsersInGameRoomAsync(gameRooms.ElementAt(i).Id);
                gameRoomsDTO.ElementAt(i).CurrentPlayers = usersInRoom.Count();
            }

            return Result<IEnumerable<GameRoomDTO>>.Ok(gameRoomsDTO);
        }

        public async Task<Result<IEnumerable<UserDTO>>> GetUsersInGameRoomAsync(Guid gameRoomId)
        {
            var checkRoom = await _gameRoomRepository.GetByIdAsync(gameRoomId);
            if (checkRoom == null)
            {
                Log.Error("Game Room {RoomId} not found.", gameRoomId);
                return Result<IEnumerable<UserDTO>>.Fail("Game Room not found.");
            }

            var users = await _sessionRepository.GetUsersInGameRoomAsync(gameRoomId);
            var usersDTO = _mapper.Map<IEnumerable<UserDTO>>(users);
            return Result<IEnumerable<UserDTO>>.Ok(usersDTO);
        }

        public async Task<Result<bool>> CreateGameRoomAsync(GameRoomCreationDTO gameRoomDTO)
        {
            var user = await _userRepository.GetByEmailAsync(gameRoomDTO.UserId);
            var gameRoom = CreateGameRoom(gameRoomDTO, user.Id);
            try
            {
                await _gameRoomRepository.CreateAsync(gameRoom);
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
                var gameRoom = await _gameRoomRepository.GetByIdAsync(gameRoomId);
                if (user == null || gameRoom == null)
                {
                    Log.Error("User or Game Room not found.");
                    return Result<bool>.Fail("User or Game Room not found.");
                }

                var usersInRoom = await _sessionRepository.GetUsersInGameRoomAsync(gameRoomId);
                if (usersInRoom.Any(userId => userId == user.Id))
                {
                    Log.Warning("User {UserId} is already in the Game Room {RoomId}.", user.Id, gameRoomId);
                    return Result<bool>.Fail("Již jste v této místnosti připojeni.");
                }
                if (usersInRoom.Count() >= gameRoom.MaxPlayers)
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

                var added = await _sessionRepository.AddUserToSessionAsync(session);
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
                var user = await _userRepository.GetByEmailAsync(request.UserEmail);
                var gameRoom = await _gameRoomRepository.GetByIdAsync(gameRoomId);
                if (user == null || gameRoom == null)
                {
                    Log.Error("User or Game Room not found.");
                    return Result<bool>.Fail("User or Game Room not found.");
                }
                var usersInRoom = await _sessionRepository.GetUsersInGameRoomAsync(gameRoomId);
                if (usersInRoom.Contains(user.Id) == false)
                {
                    Log.Error("User not in the Game Room.");
                    return Result<bool>.Fail("User not in the Game Room.");
                }
                var removed = await _sessionRepository.RemoveUserFromSessionAsync(user.Id, gameRoomId);
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
            var password = string.IsNullOrWhiteSpace(gameRoomDTO.Password) ? null : gameRoomDTO.Password;

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
