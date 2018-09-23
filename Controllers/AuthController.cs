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

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userDto){
            userDto.Username = userDto.Username.ToLower();
            //cheking the attempt to login
            var userFromRepo = await _repo.Login(userDto.Username, userDto.Password);

            if(userFromRepo == null){
                return Unauthorized();
            }

            //if a user was successfully retrieved from database
            //we begin the construction of our JWT token

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username),
            };

            var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value)
                    );
            //now that we have our key, we can build the credential
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token),
            });
        }
    }
}