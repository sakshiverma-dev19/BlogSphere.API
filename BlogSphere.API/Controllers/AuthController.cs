
using BlogSphere.API.Data;
using BlogSphere.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BlogSphereDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(BlogSphereDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return BadRequest("Username, Email and Password are required.");
            }

            bool userExists = await _context.Users.AnyAsync(
                u => u.Email == user.Email || u.Username == user.Username);

            if (userExists)
            {
                return BadRequest("Username or Email already exists.");
            }

            string password = user.PasswordHash;

            user.Id = 0;
            user.CreatedAt = DateTime.UtcNow;
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User registered successfully.",
                user.Id,
                user.Username,
                user.Email
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(User loginUser)
        {
            if (string.IsNullOrWhiteSpace(loginUser.Email) ||
                string.IsNullOrWhiteSpace(loginUser.PasswordHash))
            {
                return BadRequest("Email and Password are required.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginUser.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginUser.PasswordHash);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid email or password.");
            }

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()["Jwt:Key"]!));

            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key,
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.NameIdentifier,
                    user.Id.ToString()),
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Name,
                    user.Username),
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Email,
                    user.Email)
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Jwt:Issuer"],
                audience: HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            var tokenString =
                new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                    .WriteToken(token);

            return Ok(new
            {
                Message = "Login successful.",
                Token = tokenString,
                UserId = user.Id,
                Username = user.Username
            });
        }

    }
}
