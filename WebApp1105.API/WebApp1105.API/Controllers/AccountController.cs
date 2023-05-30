using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApp1105.API.Models;
using WebApp1105.API.Data.Models;
using WebApp1105.Models;
using WebApp1105.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using BCrypt.Net;

namespace WebApp1105.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AuthOptions _authOptions;
        private readonly IAllAccounts _allAccounts;
        private const string AuthSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme +
            "," + JwtBearerDefaults.AuthenticationScheme;

        public AccountController(
            IConfiguration configuration,
            IAllAccounts allAccounts,
            ApplicationDbContext dbContext)
        {
            _authOptions = configuration.GetSection("AuthOptions").Get<AuthOptions>();
            _allAccounts = allAccounts;
            _dbContext = dbContext;
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password, HashType.SHA512);
                Account? account = await _dbContext.Account.FirstOrDefaultAsync(p => p.UserName == model.UserName);
                if (account != null)
                {
                    if (!BCrypt.Net.BCrypt.EnhancedVerify(model.Password, account.PasswordHash, HashType.SHA512))
                        return Unauthorized();
                }
                else
                {
                    account = new()
                    {
                        UserName = model.UserName,
                        PasswordHash = passwordHash,
                        Email = "NULL"
                    };
                    _dbContext.Account.Add(account);
                    await _dbContext.SaveChangesAsync();
                }

                if (model.TypeAuth == "Cookie")
                {
                    var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, account.UserName) };
                    ClaimsIdentity claimsIdentity =
                        new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    var response = new { username = account.UserName };
                    return Ok(response);
                }
                if (model.TypeAuth == "Bearer")
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, account.UserId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Name, account.UserName),
                    };
                    var response = new
                    {
                        access_token = GenerateJwt(claims),
                        username = account.UserName
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
                var response = new { username = _dbContext.Account.FirstOrDefault(p => p.UserId == UserId).UserName };
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