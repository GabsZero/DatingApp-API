using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            userDto.Username = userDto.Username.ToLower();

            if(await _repo.UserExistes(userDto.Username)){
                return BadRequest("Username already exists!");
            }    

            //creating the new user
            var newUser = new User{
                Username = userDto.Username,
            };

            var user = await _repo.Register(newUser, userDto.Password);

            // TODO: usar o método CreatedAtRoute
            return StatusCode(201);
        }
    }
}