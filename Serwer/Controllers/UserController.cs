using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serwer.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Serwer.Controllers {
    
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase {
        
        private static User? _user;
        private readonly IUserService _userService;

        public UserController(IUserService userService) {
            _userService = userService;
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
            
            return Ok(CreateToken(_user));
         }

        /// <summary>
        /// Get information about logged in user
        /// </summary>
        /// <returns>Username and role</returns>
        [HttpGet("info"), Authorize]
        public ActionResult<object> GetMe() {
            var name = _userService.GetName();
            var role = _userService.GetRole();
            if (name == string.Empty || role == string.Empty) {
                return Unauthorized();
            }
            return Ok(new { name, role });
        }

        /// <summary>
        /// Refresh user token for another day
        /// </summary>
        /// <returns>New JWT Authorization Token</returns>
        [HttpPost("refresh"), Authorize]
        public ActionResult<string> RefreshToken() {
            DateTime expiration = _userService.GetExpirationDate();
            
            //check if it is less than two hours to expiration date
            if (expiration.Subtract(DateTime.Now).TotalHours > 2) {
                return BadRequest("The token is not yet up for renewal.");
            }

            // I should probably use refreshToken field to make sure (or make life harder for the thief)
            // that I am renewing the token for the right person.
            // For demonstration purposes, I won't make it harder to understand.
            User? userRenewal = _user; // db query
            if (userRenewal == null) return Unauthorized();
            
            return Ok(CreateToken(userRenewal));
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
            DateTime expDate = DateTime.Now.AddDays(1);
            List<Claim> claims = new() {
                new Claim(ClaimTypes.NameIdentifier, user.UID),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.AccountType),
                new Claim(ClaimTypes.Expiration, expDate.ToString())
            };
            SymmetricSecurityKey key = new(System.Text.Encoding.UTF8.GetBytes(ENVLoader.GetString("TOKEN")));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);
            JwtSecurityToken token = new(
                claims: claims,
                expires: expDate,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
