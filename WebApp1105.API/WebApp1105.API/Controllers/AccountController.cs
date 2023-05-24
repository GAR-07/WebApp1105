using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApp1105.API.Models;

namespace WebApp1105.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly AuthOptions _authOptions;
        private const string AuthSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme +
            "," + JwtBearerDefaults.AuthenticationScheme;

        public AccountController(IConfiguration configuration)
        {
            _authOptions = configuration.GetSection("AuthOptions").Get<AuthOptions>();
        }

        public List<Account> Accounts => new List<Account>
        {
            new Account
            {
                UserId = Guid.Parse("F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4"),
                UserName = "Александр",
                Password = "1234"
            },
            new Account
            {
                UserId = Guid.Parse("F1234D5E-CEB2-4faa-B6BF-329BF39FA1E4"),
                UserName = "Олег",
                Password = "1234"
            }
        };

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Accounts.FirstOrDefault(p => p.UserName == model.UserName && p.Password == model.Password);
                if (user is null) return Unauthorized();

                if (model.TypeAuth == "Cookie")
                {
                    var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName) };
                    ClaimsIdentity claimsIdentity =
                        new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    var response = new { username = user.UserName };
                    return Ok(response);
                }
                else if (model.TypeAuth == "Bearer")
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                    };
                    var response = new
                    {
                        access_token = GenerateJwt(claims),
                        username = user.UserName
                    };
                    return Json(response);
                }
            }
            return Unauthorized();
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("Cabinet")]
        public IActionResult Cabinet()
        {
            try
            {
                Guid UserId = Guid.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var response = new { username = Accounts.FirstOrDefault(p => p.UserId == UserId).UserName };
                return Json(response);
            }
            catch
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    string login = HttpContext.User.Identity.Name;
                    var response = new { username = login };
                    return Ok(response);
                }
                else
                    return Unauthorized();
            }
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return Ok();
        }

        private string GenerateJwt(List<Claim> claims)
        {
            var jwtToken = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                notBefore: DateTime.Now,
                claims: claims,
                expires: DateTime.Now.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(_authOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}