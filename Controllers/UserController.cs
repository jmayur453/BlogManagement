using BlogManagement.DataContracts;
using BlogManagement.Helpers;
using BlogManagement.Models;
using BlogManagement.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly BlogApplicationDBContext _context;
        private readonly IConfiguration _config;
        private readonly UserHelper _userHelper;
        public UserController(IConfiguration config, BlogApplicationDBContext context, UserHelper userHelper)
        {
            _config = config;
            _context = context;
            _userHelper = userHelper;
        }
        [HttpPost("login")]
        public IActionResult GenerateToken([FromBody] LoginUserRequest user)
        {
            var userDetails = _context.UserDetails.FirstOrDefault(x => x.Email == user.Email && x.IsActive && !x.IsDeleted);
            if (userDetails == null) return BadRequest("User Not Found!!!");
            var isVerified = _userHelper.VerifyPassword(userDetails, user.Password);
            if (!isVerified) return BadRequest("Invalid Password!!!");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("userid", userDetails.Id.ToString()) // Example role
            }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpiryMinutes"])),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }
        [HttpPost("register")]
        public async Task<bool> RegisterUser(RegisterUserRequest request)
        {
            var ifExist = _context.UserDetails.Any(x => x.Email == request.Email);
            if (ifExist) return false;
            var userDetail = new UserDetail
            {
                Email = request.Email,
                FullName = request.FullName,
                MobileNo = request.MobileNo,
                Password = request.Password
            };
            userDetail.Password = _userHelper.EncryptPassword(userDetail);
            await _context.AddAsync(userDetail);
            if (await _context.SaveChangesAsync() > 0)
            {
                return true;
            }
            return false;
        }
    }
}
