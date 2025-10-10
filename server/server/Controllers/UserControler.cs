using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Serilog;
using server.Models.DB;
using server.Services.DbServices.Interfaces;

namespace server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserControler : ControllerBase
    {
        private readonly IUserDbServices _userDbServices;

        public UserControler(IUserDbServices userDbServices)
        {
            _userDbServices = userDbServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                Log.Debug("Get allusers");
                var users = await _userDbServices.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userDbServices.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
