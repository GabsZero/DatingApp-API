using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            if(await _repo.UserExists(userDto.Username)){
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userDto){
            //cheking the attempt to login
            var token = await _repo.Login(userDto.Username, userDto.Password);

            if(token == null){
                return Unauthorized();
            }

            return Ok(new {
                token = token,
            });
        }
    }
}