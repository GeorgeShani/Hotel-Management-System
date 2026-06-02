using BackEnd.DTOs.Auth;
using BackEnd.Interfaces;
using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackEnd.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // 1. Check whether a user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return new AuthResponseDto { IsSuccess = false, Message = "A user with this email already exists." };

            // 2. Create the new user
            var user = new ApplicationUser
            {
                UserName = registerDto.Email, // use the email for login
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return new AuthResponseDto { IsSuccess = false, Message = "Registration failed. Check the password complexity." };

            // 3. Ensure the role exists and assign it (create it first if it does not exist)
            if (!await _roleManager.RoleExistsAsync(registerDto.Role))
                await _roleManager.CreateAsync(new IdentityRole(registerDto.Role));

            await _userManager.AddToRoleAsync(user, registerDto.Role);

            return new AuthResponseDto { IsSuccess = true, Message = "User registered successfully!" };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // 1. Find the user by email
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };

            // 2. Verify the password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password." };

            // 3. If everything is fine, generate the token
            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Successful authentication",
                Token = token
            };
        }

        // --- Internal method to create the token ---
        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];

            // Prepare the information (Claims) to be embedded in the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // unique token ID
                new Claim("FirstName", user.FirstName)
            };

            // Add the user's roles to the token
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Sign with our secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
