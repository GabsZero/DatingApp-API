using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
            _repo = repo;
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // TODO: validate request

            username = username.ToLower();

            if(await _repo.UserExistes(username)){
                return BadRequest("Username already exists!");
            }    

            //creating the new user
            var newUser = new User{
                Username = username,
            };

            var user = await _repo.Register(newUser, password);

            // TODO: usar o método CreatedAtRoute
            return StatusCode(201);
        }
    }
}