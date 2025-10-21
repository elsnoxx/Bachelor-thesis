using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Utilities;
using server.Models.DTO;
using server.Services.DbServices.Interfaces;
using System.Security.AccessControl;

namespace server.Controllers
{
    [ApiController]
    [Route("api/gamerooms/")]
    public class GameRoomControler : ControllerBase
    {
        private readonly IGameRoomService _gameRoomService;
        public GameRoomControler(IGameRoomService gameRoomService)
        {
            _gameRoomService = gameRoomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGameRooms([FromQuery] string gameType)
        {
            var gameRooms = await _gameRoomService.GetGameRoomsListAsync(gameType);
            return Ok(gameRooms);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersInGameRoom(Guid gameRoomId)
        {
            var users = await _gameRoomService.GetUsersGameRoomAsync(gameRoomId);

            if (!users.Success)
            {
                return BadRequest(users.Data);
            }

            return Ok();
        }


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

        [HttpPost("{GameRoomId}/join")]
        public async Task<IActionResult> JoinGameRoom(Guid GameRoomId, [FromBody] JoinRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data");
            }

            var result = await _gameRoomService.JoinGameRoomAsync(GameRoomId, request);

            if (!result.Success)
            {
                return BadRequest(result.Data);
            }

            return Ok($"You join room no. {GameRoomId}");
        }

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

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartGameRoom(Guid id, [FromBody] JoinRoomRequest request)
        {
            return Ok($"Hello you try to start romm {id}");
        }
    }
}
