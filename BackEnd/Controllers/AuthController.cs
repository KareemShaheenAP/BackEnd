using BackEnd.DTO.Auth;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthController(IConfiguration configuration, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        /// <summary>
        /// Registers a new user account with the specified registration details.
        /// </summary>
        /// <remarks>This endpoint allows anonymous access. If the provided email is already associated
        /// with an existing user, the registration will fail. The user is assigned the specified role upon successful
        /// registration.</remarks>
        /// <param name="dto">An object containing the user's registration information, including email, password, first name, last name,
        /// and role. All required fields must be provided and valid.</param>
        /// <returns>An IActionResult indicating the result of the registration operation. Returns 200 OK if registration is
        /// successful; otherwise, returns 400 Bad Request with error details.</returns>
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Register dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var user = new AppUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Registration failed.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            //add role
            await SeedRoles(_roleManager);
            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new { message = "User registered successfully." });
        }
        /// <summary>
        /// Authenticates a user with the provided credentials and returns a JWT token if successful.
        /// </summary>
        /// <remarks>This endpoint does not require authentication. The returned JWT token can be used to
        /// authorize subsequent requests. If the credentials are invalid, the response will indicate an authentication
        /// failure without specifying which field was incorrect.</remarks>
        /// <param name="dto">The login credentials containing the user's email and password.</param>
        /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="AuthResponse"/> with the JWT token and user
        /// information if authentication is successful; otherwise, an error response indicating invalid credentials or
        /// validation errors.</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] Login dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAt) = GenerateJwtToken(user, roles);

            return Ok(new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                Email = user.Email ?? string.Empty
            });
        }

        /// <summary>
        /// Ensures that the default application roles exist in the identity store, creating them if they do not already
        /// exist.
        /// </summary>
        /// <remarks>This method is typically called during application startup to ensure required roles
        /// such as "Admin", "User", and "Manager" are present. If a role already exists, it is not recreated.</remarks>
        /// <param name="roleManager">The role manager used to check for and create roles in the identity store. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = ["Admin", "User", "Manager"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        /// <summary>
        /// Generates a JSON Web Token (JWT) for the specified user and roles, including standard claims and an
        /// expiration date.
        /// </summary>
        /// <remarks>The token includes standard claims such as subject, email, and unique identifier, as
        /// well as all provided roles. The expiration period is determined by the 'JWT:DurationInDays' configuration
        /// value, defaulting to 7 days if not specified.</remarks>
        /// <param name="user">The user for whom the JWT will be generated. The user's identifier and email are included as claims in the
        /// token.</param>
        /// <param name="roles">A list of role names to be included as role claims in the JWT. Each role in the list is added as a separate
        /// claim.</param>
        /// <returns>A tuple containing the generated JWT as a string and the UTC expiration date and time of the token.</returns>
        /// <exception cref="InvalidOperationException">Thrown if required JWT configuration values ('JWT:Key', 'JWT:Issuer', or 'JWT:Audience') are missing.</exception>
        private (string token, DateTime expiresAt) GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var jwtKey = _configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT:Key is missing.");
            var jwtIssuer = _configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer is missing.");
            var jwtAudience = _configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience is missing.");

            var durationInDaysString = _configuration["JWT:DurationInDays"] ?? "7";
            var durationInDays = int.TryParse(durationInDaysString, out var parsed) ? parsed : 7;
            var expiresAt = DateTime.UtcNow.AddDays(durationInDays);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiresAt);
        }
    }
}
