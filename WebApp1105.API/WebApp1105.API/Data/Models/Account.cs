using System.ComponentModel.DataAnnotations;

namespace WebApp1105.API.Data.Models
{
    public class Account
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
