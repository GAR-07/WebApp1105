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
                Account? account = await _dbContext.Accounts.FirstOrDefaultAsync(p => p.UserName == model.UserName);
                if (account != null)
                {
                    if (!BCrypt.Net.BCrypt.EnhancedVerify(model.Password, account.PasswordHash, HashType.SHA512))
                    {
                        return Unauthorized("Password is not valid");
                    }
                }
                else
                {
                    account = new()
                    {
                        UserName = model.UserName,
                        PasswordHash = passwordHash,
                        Email = "NULL"
                    };
                    _dbContext.Accounts.Add(account);
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
                    var response = new 
                    { 
                        username = account.UserName 
                    };
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
                        accessToken = GenerateJwt(claims),
                        userName = account.UserName
                    };
                    return Ok(response);
                }
            }
            return BadRequest("Invalid model object");
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthSchemes)]
        [Route("AccountConfirm")]
        public IActionResult AccountConfirm()
        {
            try
            {
                Guid UserId = Guid.Parse(User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var response = new {
                    userId = UserId,
                    userName = _dbContext.Accounts.FirstOrDefault(p => p.UserId == UserId).UserName
                };
                return Ok(response);
            }
            catch
            {
                var response = new { 
                    userId = _dbContext.Accounts.FirstOrDefault(p => p.UserName == HttpContext.User.Identity.Name).UserId,
                    userName = HttpContext.User.Identity.Name
                };
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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