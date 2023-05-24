using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApp1105.API.Models
{
    public class AuthOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; } 
        public string SecretKey { get; set; }
        public SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
    }
}
