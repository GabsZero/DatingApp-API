using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _db;
        private readonly IConfiguration _config;

        public AuthRepository(DataContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }
        public async Task<string> Login(string username, string password)
        {
            username = username.ToLower();

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == username);

            if(user == null){
                return null;
            }

            // checking if the hashs match
            if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)){
                return null;
            }

            //if a user was successfully retrieved from database
            //we begin the construction of our JWT token

            var token = this.ConfigureToken(user);

            return token;

        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                // we need to loop in the hash and check if there's anything different between them
                for(int i = 0; i < computedHash.Length; i++){
                    if(computedHash[i] != passwordHash[i]){
                        return false;
                    }
                }
            }

            //if we got here, the hash match!
            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            user.Username = user.Username.ToLower();
            byte[] passwordHash, passwordSalt;

            // creating hashs
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            // since they were passed as reference, we can use them now
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return user;
        }

        private string ConfigureToken(User user){
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
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

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            var token = tokenHandler.WriteToken(securityToken);

            return token;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            //
            username = username.ToLower();

            if(await _db.Users.AnyAsync(x => x.Username == username)){
                //found that username in our database
                return true;
            }

            //no matching username
            return false;
        }
    }
}