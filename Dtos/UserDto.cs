using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }    
        
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "Password must have 4 to 8 characters")]
        public string Password { get; set; }
    }
}