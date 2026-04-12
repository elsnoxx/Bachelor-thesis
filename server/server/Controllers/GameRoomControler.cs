using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Utilities;
using server.Models.DTO;
using server.Services.DbServices.Interfaces;
using System.Security.AccessControl;

namespace server.Controllers
{
    /// <summary>
    /// Controller managing the lifecycle of game rooms, including creation, 
    /// joining, and player management.
    /// All endpoints require a valid JWT token.
    /// </summary>
    [ApiController]
    [Route("api/gamerooms")]
    [Authorize] // Ensures only authenticated users can manage game rooms
    public class GameRoomControler : ControllerBase
    {
        private readonly IGameRoomService _gameRoomService;
        public GameRoomControler(IGameRoomService gameRoomService)
        {
            _gameRoomService = gameRoomService;
        }

        /// <summary>
        /// Retrieves a list of available game rooms, optionally filtered by game type.
        /// </summary>
        /// <param name="gameType">Optional filter for specific game mode (e.g., 'balance').</param>
        /// <returns>A list of game room summaries.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllGameRooms([FromQuery] string gameType)
        {
            var gameRooms = await _gameRoomService.GetGameRoomsListAsync(gameType);
            return Ok(gameRooms);
        }

        /// <summary>
        /// Gets the list of users currently present in a specific game room.
        /// </summary>
        /// <param name="gameRoomId">The unique identifier of the game room.</param>
        [HttpGet("{gameRoomId}/users")]
        public async Task<IActionResult> GetUsersInGameRoom(Guid gameRoomId)
        {
            var users = await _gameRoomService.GetUsersInGameRoomAsync(gameRoomId);

            if (!users.Success)
            {
                return BadRequest(users.Data);
            }

            return Ok();
        }


        /// <summary>
        /// Creates a new game room for players to join.
        /// </summary>
        /// <param name="gameRoom">DTO containing initial game room configuration.</param>
        /// <response code="200">Game room successfully created.</response>
        /// <response code="400">Invalid input data or creation failed.</response>
        [HttpPost]
        public async Task<IActionResult> CreateGameRooms([FromBody] GameRoomCreationDTO gameRoom)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data");
            }

            var result = await _gameRoomService.CreateGameRoomAsync(gameRoom);
            if (!result.Success)
            {
                return BadRequest(result.Data);
            }
            return Ok("You try create game room");
        }

        /// <summary>
        /// Adds a user to an existing game room.
        /// </summary>
        /// <param name="gameRoomId">The ID of the room to join.</param>
        /// <param name="request">Request details including user identification.</param>
        [HttpPost("{gameRoomId}/join")]
        public async Task<IActionResult> JoinGameRoom(Guid GameRoomId, [FromBody] JoinRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data");
            }

            var result = await _gameRoomService.JoinGameRoomAsync(GameRoomId, request);

            if (!result.Success)
            {
                return BadRequest($"User already in the Game Room.");
            }

            return Ok($"You join room no. {GameRoomId}");
        }

        /// <summary>
        /// Removes a user from a game room.
        /// </summary>
        /// <param name="id">The ID of the room to leave.</param>
        /// <param name="request">Identification of the user leaving.</param>
        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveGameRoom(Guid id, [FromBody] LeaveRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data");
            }

            var result = await _gameRoomService.LeaveGameRoomAsync(id, request); 

            if (!result.Success)
            {
                return BadRequest(result.Data);
            }

            return Ok($"You leave romm no. {id}");
        }

        /// <summary>
        /// Transitions the room state from 'Waiting' to 'InGame'.
        /// </summary>
        /// <param name="id">Room ID to start.</param>
        /// <param name="request">Information about the user initiating the start (typically the host).</param>
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartGameRoom(Guid id, [FromBody] JoinRoomRequest request)
        {
            return Ok($"Hello you try to start romm {id}");
        }
    }
}
