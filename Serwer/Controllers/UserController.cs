using IdGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Serwer.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Serwer.Controllers {
    
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase {

        private readonly IUserService _userService;
        private static readonly SqliteConnection _connection = new("Data Source=database/serwis.sqlite");

        private readonly DateTime epoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly IdStructure structure = new(41, 10, 12);
        private readonly IdGeneratorOptions options;
        private readonly IdGenerator generator;

        public UserController(IUserService userService) {
            _userService = userService;
            options = new(structure, new DefaultTimeSource(epoch));
            generator = new(0, options);

            _connection.Open();
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="request">JSON with Username, Password, Name and Surname string fields.</param>
        /// <returns>JWT Authorization Token</returns>
        [HttpPost("register")]
        public ActionResult<string> Register(UserRegisterDTO request) {
            if (request.Password.Length < 8) {
                return BadRequest("Password must be at least 8 characters long");
            }
            if (request.Username.Length < 4) {
                return BadRequest("Username must be at least 4 characters long");
            }
            if (request.Name.Length < 2) {
                return BadRequest("Name must be at least 2 characters long");
            }
            if (request.Surname.Length < 2) {
                return BadRequest("Surname must be at least 2 characters long");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            SqliteCommand command = _connection.CreateCommand();
            command.CommandText =
                @"
                    SELECT name
                    FROM Users
                    WHERE name = $username
                ";
            command.Parameters.AddWithValue("username", request.Username);
            var result = command.ExecuteScalar();
            if (result != null) {
                return BadRequest("Username already exists");
            }

            long id = generator.CreateId();
            command.CommandText =
                @"
                    INSERT INTO Users (uid, name, surname, login, password, passwordSalt, account_type)
                    VALUES ($uid, $name, $surname, $login, $password, $passwordSalt, $account_type)
                ";
            command.Parameters.AddWithValue("uid", id);
            command.Parameters.AddWithValue("name", request.Name);
            command.Parameters.AddWithValue("surname", request.Surname);
            command.Parameters.AddWithValue("login", request.Username);
            command.Parameters.AddWithValue("password", passwordHash);
            command.Parameters.AddWithValue("passwordSalt", passwordSalt);
            command.Parameters.AddWithValue("account_type", "Customer");
            try {
                command.ExecuteNonQuery();
            } catch (SqliteException e) {
                return BadRequest(e.Message);
            }
            User user = new() {
                UID = id.ToString(),
                Login = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Name = request.Name,
                Surname = request.Surname,
                AccountType = "Customer"
            };

            return Ok(CreateToken(user));
        }

        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="request">JSON with Username and Password</param>
        /// <returns>JWT Authorization Token</returns>
        [HttpPost("login")]
        public ActionResult<string> Login(UserLoginDTO request) {
            SqliteCommand sqliteCommand = _connection.CreateCommand();
            sqliteCommand.CommandText =
                @"
                    SELECT uid, name, surname, login, password, passwordSalt, account_type
                    FROM Users
                    WHERE login = $login
                ";
            sqliteCommand.Parameters.AddWithValue("login", request.Username);
            SqliteDataReader reader = sqliteCommand.ExecuteReader();
            if (!reader.Read()) {
                return BadRequest("Invalid username or password");
            }
            User user = new() {
                UID = reader.GetInt64(0).ToString(),
                Name = reader.GetString(1),
                Surname = reader.GetString(2),
                Login = reader.GetString(3),
                PasswordHash = (byte[])reader.GetValue(4),
                PasswordSalt = (byte[])reader.GetValue(5),
                AccountType = reader.GetString(6)
            };
            
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt)) {
                return BadRequest("Wrong password");
            }
            
            return Ok(CreateToken(user));
         }

        /// <summary>
        /// Get information about logged in user
        /// </summary>
        /// <returns>Username and role</returns>
        [HttpGet("info"), Authorize]
        public ActionResult<object> GetUserInfo() {
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
            //check if already not expired
            if (expiration < DateTime.Now) {
                return Unauthorized();
            }
            //check if it is less than two hours to expiration date
            if (expiration.Subtract(DateTime.Now).TotalHours > 2) {
                return BadRequest("The token is not yet up for renewal.");
            }

            string username = _userService.GetName();
            SqliteCommand sqliteCommand = _connection.CreateCommand();
            sqliteCommand.CommandText =
                @"
                    SELECT uid, login, account_type
                    FROM Users
                    WHERE login = $login
                ";
            sqliteCommand.Parameters.AddWithValue("login", username);
            SqliteDataReader reader = sqliteCommand.ExecuteReader();
            if (!reader.Read()) {
                return BadRequest("Invalid username or password");
            }
            User userRenewal = new() {
                UID = reader.GetInt64(0).ToString(),
                Login = reader.GetString(1),
                AccountType = reader.GetString(2)
            };

            // I should probably use refreshToken field to make sure (or make life harder for the thief)
            // that I am renewing the token for the right person.
            // For demonstration purposes, I won't make it harder to understand.
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

        private static string CreateToken(User user) {
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
