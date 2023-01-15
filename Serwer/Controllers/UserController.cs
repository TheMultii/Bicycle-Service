using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Serwer.Controllers {
    
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase {

        private static User? _user;
        private IConfiguration _configuration;

        public UserController(IConfiguration configuration) {
            _configuration = configuration;
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="request">JSON with Username, Password, Name and Surname string fields.</param>
        /// <returns>Registered User</returns>
        [HttpPost("register")]
        public ActionResult<User> Register(UserRegisterDTO request) {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User {
                UID = "vqtwdvqwydubqwudbq",
                Login = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Name = request.Name,
                Surname = request.Surname,
                AccountType = "Customer"
            };
            _user = user;
            return Ok(user);
        }

        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="request">JSON with Username and Password</param>
        /// <returns>JWT Authorization Token</returns>
        [HttpPost("login")]
        public ActionResult<string> Login(UserLoginDTO request) {
            if (_user == null) {
                return NotFound("User not found");
            } //TODO | TEMP
            if (_user.PasswordHash == null || _user.PasswordSalt == null) {
                return NotFound("User not found [2]");
            } //TODO | TEMP
            if (_user.Login != request.Username) {
                return BadRequest("User not found");
            }
            if (!VerifyPasswordHash(request.Password, _user.PasswordHash, _user.PasswordSalt)) {
                return BadRequest("Wrong password");
            }
            string jwtToken = CreateToken(_user);
            return Ok(jwtToken);
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            using HMACSHA512 hmac = new();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt) {
            using HMACSHA512 hmac = new(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private string CreateToken(User user) {
            List<Claim> claims = new() {
                new Claim(ClaimTypes.NameIdentifier, user.UID),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.AccountType)
            };
            SymmetricSecurityKey key = new(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value ?? throw new InvalidOperationException()));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);
            JwtSecurityToken token = new(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
