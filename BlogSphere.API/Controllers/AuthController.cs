
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogSphere.API.Data;
using BlogSphere.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlogSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BlogSphereDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthController(
            BlogSphereDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return BadRequest(
                    "Username, Email and Password are required.");
            }

            user.Username = user.Username.Trim();
            user.Email = user.Email.Trim();

            if (user.Username.Length > 100)
            {
                return BadRequest(
                    "Username cannot exceed 100 characters.");
            }

            if (user.Email.Length > 255)
            {
                return BadRequest(
                    "Email cannot exceed 255 characters.");
            }

            if (user.PasswordHash.Length < 8)
            {
                return BadRequest(
                    "Password must be at least 8 characters long.");
            }

            bool userExists = await _context.Users.AnyAsync(
                u => u.Email == user.Email ||
                     u.Username == user.Username);

            if (userExists)
            {
                return Conflict("Username or Email already exists.");
            }

            string password = user.PasswordHash;

            user.Id = 0;
            user.CreatedAt = DateTime.UtcNow;
            user.PasswordHash =
                _passwordHasher.HashPassword(user, password);

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

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] User loginUser)
        {
            if (loginUser == null ||
                string.IsNullOrWhiteSpace(loginUser.Email) ||
                string.IsNullOrWhiteSpace(loginUser.PasswordHash))
            {
                return BadRequest(
                    "Email and Password are required.");
            }

            string email = loginUser.Email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                loginUser.PasswordHash);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            string? jwtKey = _configuration["Jwt:Key"];
            string? issuer = _configuration["Jwt:Issuer"];
            string? audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "JWT configuration is missing.");
            }

            if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "JWT key configuration is invalid.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Email,
                    user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
             expires: DateTime.UtcNow.AddMinutes(
    Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: credentials);

            string tokenString =
                new JwtSecurityTokenHandler().WriteToken(token);

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
