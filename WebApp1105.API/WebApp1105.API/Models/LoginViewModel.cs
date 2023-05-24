using System.ComponentModel.DataAnnotations;

namespace WebApp1105.API.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string TypeAuth { get; set; }
    }
}