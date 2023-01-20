using IdGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Serwer.Services;

namespace Serwer.Controllers {

    [Route("api/service")]
    [ApiController]
    public class SerwisRowerowyController : ControllerBase {
        private static readonly SqliteConnection _connection = new("Data Source=database/serwis.sqlite");
        private readonly IUserService _userService;

        private readonly DateTime epoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly IdStructure structure = new(41, 10, 12);
        private readonly IdGeneratorOptions options;
        private readonly IdGenerator generator;

        public SerwisRowerowyController(IUserService userService) {
            _userService = userService;
            
            options = new(structure, new DefaultTimeSource(epoch));
            generator = new(0, options);
            
            _connection.Open();
        }

        private static User GetUserByUID(long uid) {
            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT uid, name, surname, account_type FROM Users WHERE uid = @uid";
            command.Parameters.AddWithValue("@uid", uid);
            SqliteDataReader reader = command.ExecuteReader();
            if (reader.Read()) {
                long user_uid = reader.GetInt64(0);
                string user_name = reader.GetString(1);
                string user_surname = reader.GetString(2);
                string user_accountType = reader.GetString(3);

                return new User {
                    UID = user_uid.ToString(),
                    Name = user_name,
                    Surname = user_surname,
                    AccountType = user_accountType
                };
            }
            throw new Exception("User not found");
        }

        ~SerwisRowerowyController() {
            _connection.Close();
        }

        /// <summary>
        /// Get all bikes
        /// </summary>
        /// <returns>List of bikes</returns>
        [HttpGet("bicycles")]
        [Authorize]
        public ActionResult<IEnumerable<Rower>> GetBicycles() {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" && permission != "shop") return Unauthorized();
            
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM Bicycles";
            var reader = command.ExecuteReader();
            var bicycles = new List<Rower>();
            while (reader.Read()) {
                long uid = reader.GetInt64(0);
                long owner_id = reader.GetInt64(1);
                User owner = GetUserByUID(owner_id);
                string brand = reader.GetString(2);
                string model = reader.GetString(3);
                string type = reader.GetString(4);
                double price = reader.GetDouble(5);
                var status = new List<RowerStatus>();

                var status_command = _connection.CreateCommand();
                status_command.CommandText = "SELECT * FROM OrderStatuses WHERE bicycle = @bicycle_uid";
                status_command.Parameters.AddWithValue("@bicycle_uid", uid);
                var status_reader = status_command.ExecuteReader();
                while (status_reader.Read()) {
                    long status_uid = status_reader.GetInt64(0);
                    long changed_by_id = status_reader.GetInt64(2);
                    User changed_by = GetUserByUID(changed_by_id);
                    string status_name = status_reader.GetString(3);
                    status.Add(new RowerStatus(status_uid, changed_by, status_name));
                }

                bicycles.Add(new Rower(uid, owner, brand, model, type, price, status));
            }
            return Ok(bicycles);
        }


        /// <summary>
        /// Get bike by ID
        /// </summary>
        /// <param name="uid">UID of bike</param>
        /// <returns>Bike if exists</returns>
        [HttpGet("bicycles/{uid}")]
        [Authorize]
        public ActionResult<Rower> GetBicycle(long uid) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" && permission != "shop") return Unauthorized();
            
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM Bicycles WHERE uid = @uid";
            command.Parameters.AddWithValue("@uid", uid);
            var reader = command.ExecuteReader();
            if (reader.Read()) {
                long owner_id = reader.GetInt64(1);
                User owner = GetUserByUID(owner_id);
                string brand = reader.GetString(2);
                string model = reader.GetString(3);
                string type = reader.GetString(4);
                double price = reader.GetDouble(5);
                var status = new List<RowerStatus>();

                var status_command = _connection.CreateCommand();
                status_command.CommandText = "SELECT * FROM OrderStatuses WHERE bicycle = @bicycle_uid";
                status_command.Parameters.AddWithValue("@bicycle_uid", uid);
                var status_reader = status_command.ExecuteReader();
                while (status_reader.Read()) {
                    long status_uid = status_reader.GetInt64(0);
                    long changed_by_id = status_reader.GetInt64(2);
                    User changed_by = GetUserByUID(changed_by_id);
                    string status_name = status_reader.GetString(3);
                    status.Add(new RowerStatus(status_uid, changed_by, status_name));
                }
                return Ok(new Rower(uid, owner, brand, model, type, price, status));
            }
            return NotFound();
        }

        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="request">User and bike</param>
        /// <returns>Confirmation of order</returns>
        [HttpPost("bicycle")]
        [Authorize]
        public ActionResult<RowerReturnable> CreateOrder(RowerDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission == "service" || permission == "shop") return Unauthorized();
            
            if (request.Brand == null || request.Model == null || request.Type == null || request.Price == 0.0) {
                return BadRequest();
            }

            string user_name = _userService.GetName();
            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT uid FROM Users WHERE login = @name";
            command.Parameters.AddWithValue("@name", user_name);
            SqliteDataReader reader = command.ExecuteReader();
            if (reader.Read()) {
                long user_uid = reader.GetInt64(0);
                long uid = generator.CreateId();
                List<string> status = new();
                
                command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO Bicycles (uid, owner, brand, model, type, price) VALUES (@uid, @owner, @brand, @model, @type, @price)";
                command.Parameters.AddWithValue("@uid", uid);
                command.Parameters.AddWithValue("@owner", user_uid);
                command.Parameters.AddWithValue("@brand", request.Brand);
                command.Parameters.AddWithValue("@model", request.Model);
                command.Parameters.AddWithValue("@type", request.Type);
                command.Parameters.AddWithValue("@price", request.Price);
                command.ExecuteNonQuery();

                status.Add("Nowe zamówienie");

                command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO OrderStatuses (uid, bicycle, changed_by, status) VALUES (@uid, @bicycle, @changed_by, @status)";
                long status_uid = generator.CreateId();
                command.Parameters.AddWithValue("@uid", status_uid);
                command.Parameters.AddWithValue("@bicycle", uid);
                command.Parameters.AddWithValue("@changed_by", user_uid);
                command.Parameters.AddWithValue("@status", status[0]);
                command.ExecuteNonQuery();

                return Ok(new RowerReturnable {
                    UID = uid.ToString(),
                    OwnerUID = user_uid,
                    Brand = request.Brand,
                    Model = request.Model,
                    Type = request.Type,
                    Price = request.Price,
                    Status = status
                });
            }
            return NotFound();
        }

        /// <summary>
        /// Update an order
        /// </summary>
        /// <param name="uid">UID of an order</param>
        /// <param name="request">Order JSON</param>
        /// <returns>Modified order</returns>
        [HttpPut("orders/{uid}")]
        [Authorize]
        public ActionResult<RowerReturnable> UpdateOrder(long uid, RowerDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" && permission != "shop") return Unauthorized();

            SqliteCommand command = _connection.CreateCommand();
            command.CommandText =
                @"
                    UPDATE Bicycles
                    SET brand = @brand, model = @model, type = @type, price = @price
                    WHERE uid = @uid    
                ";
            command.Parameters.AddWithValue("@uid", uid);
            command.Parameters.AddWithValue("@brand", request.Brand);
            command.Parameters.AddWithValue("@model", request.Model);
            command.Parameters.AddWithValue("@type", request.Type);
            command.Parameters.AddWithValue("@price", request.Price);
            command.ExecuteNonQuery();

            command = _connection.CreateCommand();
            command.CommandText = "SELECT status FROM OrderStatuses WHERE bicycle = @uid";
            command.Parameters.AddWithValue("@uid", uid);
            SqliteDataReader reader = command.ExecuteReader();
            List<string> status = new();
            while (reader.Read()) {
                status.Add(reader.GetString(0));
            }

            RowerReturnable returnable = new() {
                UID = uid.ToString(),
                Brand = request.Brand,
                Model = request.Model,
                Type = request.Type,
                Price = request.Price,
                Status = status
            };
            return Ok(returnable);
        }

        /// <summary>
        /// Set current order status
        /// </summary>
        /// <param name="uid">UID of an order</param>
        /// <param name="request">JSON with order status</param>
        /// <returns>Modified order</returns>
        [HttpPut("order/{uid}/status")]
        [Authorize]
        public ActionResult<RowerReturnable> UpdateOrderStatus(long uid, RowerStatusDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" && permission != "shop") return Unauthorized();

            if (request.Status == "Zrealizowane" && permission != "shop") return Unauthorized("You don't have permission to change status to 'Zrealizowane'");
            if (request.Status == "Oczekujące" && permission != "service") return Unauthorized("You don't have permission to change status to 'Oczekujące'");
            if (request.Status == "W trakcie realizacji" && permission != "service") return Unauthorized("You don't have permission to change status to 'W trakcie realizacji'");

            List<string> validStatuses = new() { "Zrealizowane", "Oczekujące", "W trakcie realizacji", "Nowe zamówienie" };
            if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status");

            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT status FROM OrderStatuses WHERE bicycle = @uid";
            command.Parameters.AddWithValue("@uid", uid);
            SqliteDataReader reader = command.ExecuteReader();
            List<string> status = new();
            while (reader.Read()) {
                status.Add(reader.GetString(0));
            }
            status.Add(request.Status);

            command = _connection.CreateCommand();
            command.CommandText = "SELECT uid from Users WHERE login = @name";
            command.Parameters.AddWithValue("@name", _userService.GetName());
            reader = command.ExecuteReader();
            if (reader.Read()) {
                long user_uid = reader.GetInt64(0);

                command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO OrderStatuses (uid, bicycle, changed_by, status) VALUES (@uid, @bicycle, @changed_by, @status)";
                long status_uid = generator.CreateId();
                command.Parameters.AddWithValue("@uid", status_uid);
                command.Parameters.AddWithValue("@bicycle", uid);
                command.Parameters.AddWithValue("@changed_by", user_uid);
                command.Parameters.AddWithValue("@status", request.Status);
                command.ExecuteNonQuery();

                string brand, model, type;
                double price;
                command = _connection.CreateCommand();
                command.CommandText = "SELECT brand, model, type, price FROM Bicycles WHERE uid = @uid";
                command.Parameters.AddWithValue("@uid", uid);
                reader = command.ExecuteReader();
                if (reader.Read()) {
                    brand = reader.GetString(0);
                    model = reader.GetString(1);
                    type = reader.GetString(2);
                    price = reader.GetDouble(3);
                } else {
                    return NotFound();
                }

                RowerReturnable returnable = new() {
                    UID = uid.ToString(),
                    Brand = brand,
                    Model = model,
                    Type = type,
                    Price = price,
                    Status = status
                };
                return Ok(returnable);
            }
            return NotFound();
        }

        /// <summary>
        /// Get all user orders
        /// </summary>
        /// <returns>List of user orders</returns>
        [HttpGet("/my-orders")]
        [Authorize(Roles = "Customer")]
        public ActionResult<IEnumerable<RowerReturnable>> GetMyOrders() {
            string _name = _userService.GetName();
            List<RowerReturnable> returnable = new();

            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT uid FROM Users WHERE login = @name";
            command.Parameters.AddWithValue("@name", _name);
            SqliteDataReader reader = command.ExecuteReader();
            if (reader.Read()) {
                long uid = reader.GetInt64(0);

                command = _connection.CreateCommand();
                command.CommandText = "SELECT uid, brand, model, type, price FROM Bicycles WHERE owner = @uid";
                command.Parameters.AddWithValue("@uid", uid);
                reader = command.ExecuteReader();
                while (reader.Read()) {
                    long bicycle_uid = reader.GetInt64(0);
                    string brand = reader.GetString(1);
                    string model = reader.GetString(2);
                    string type = reader.GetString(3);
                    double price = reader.GetDouble(4);

                    command = _connection.CreateCommand();
                    command.CommandText = "SELECT status FROM OrderStatuses WHERE bicycle = @uid";
                    command.Parameters.AddWithValue("@uid", bicycle_uid);
                    SqliteDataReader status_reader = command.ExecuteReader();
                    List<string> status = new();
                    while (status_reader.Read()) {
                        status.Add(status_reader.GetString(0));
                    }

                    RowerReturnable rower = new() {
                        UID = bicycle_uid.ToString(),
                        OwnerUID = uid,
                        Brand = brand,
                        Model = model,
                        Type = type,
                        Price = price,
                        Status = status
                    };
                    returnable.Add(rower);
                }
            }
            return Ok(returnable);
        }

    }
}
