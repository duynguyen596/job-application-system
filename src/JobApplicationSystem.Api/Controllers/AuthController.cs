using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobApplicationSystem.Application.Features.Authentication.Dtos;

namespace JobApplicationSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null) return BadRequest(new { Message = "User exists." });
            var newUser = new IdentityUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!result.Succeeded) return BadRequest(new { Message = "User creation failed.", Errors = result.Errors });
            if (await _roleManager.RoleExistsAsync("Candidate"))
                await _userManager.AddToRoleAsync(newUser, "Candidate"); // Assign default role
            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
        {
            if (!ModelState.IsValid) return Unauthorized(new { Message = "Invalid credentials." });
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized(new { Message = "Invalid credentials." });
            
            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var token = GenerateJwtToken(authClaims);
            return Ok(new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo,
                UserId = user.Id,
                Email = user.Email, Roles = userRoles
            });
        }

        private JwtSecurityToken GenerateJwtToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var tokenValidityInMinutes = _configuration.GetValue<int>("JwtSettings:DurationInMinutes", 60);
            var token = new JwtSecurityToken(issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddMinutes(tokenValidityInMinutes), claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
            return token;
        }
    }

// --- DTOs needed by AuthController ---
}

namespace JobApplicationSystem.Application.Features.Authentication.Dtos
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterUserDto
    {
        [Required] [EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }
}

namespace JobApplicationSystem.Application.Features.Authentication.Dtos
{
    using System.ComponentModel.DataAnnotations;

    public class LoginUserDto
    {
        [Required] [EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }
}

namespace JobApplicationSystem.Application.Features.Authentication.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = "";
        public DateTime Expiration { get; set; }
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public IList<string> Roles { get; set; } = new List<string>();
    }
}