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

        public SerwisRowerowyController(IUserService userService) {
            _userService = userService;
            _connection.Open();
        }

        private User _getUserByUID(long uid) {
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
            if (permission != "service" || permission != "shop") return Unauthorized();
            
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM rowery";
            var reader = command.ExecuteReader();
            var bicycles = new List<Rower>();
            while (reader.Read()) {
                long uid = reader.GetInt64(0);
                long owner_id = reader.GetInt64(1);
                User owner = _getUserByUID(owner_id);
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
                    long changed_by_id = status_reader.GetInt64(1);
                    User changed_by = _getUserByUID(changed_by_id);
                    string status_name = status_reader.GetString(2);
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
        [HttpGet("bicycles/{id}")]
        [Authorize]
        public ActionResult<Rower> GetBicycle(long uid) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();
            
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT * FROM Bicycles WHERE uid = @uid";
            command.Parameters.AddWithValue("@uid", uid);
            var reader = command.ExecuteReader();
            if (reader.Read()) {
                long owner_id = reader.GetInt64(1);
                User owner = _getUserByUID(owner_id);
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
                    long changed_by_id = status_reader.GetInt64(1);
                    User changed_by = _getUserByUID(changed_by_id);
                    string status_name = status_reader.GetString(2);
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
            if (permission != "service" || permission != "shop") return Unauthorized();
            
            if (request.Brand == null || request.Model == null || request.Type == null || request.Price == 0.0) {
                return BadRequest();
            }

            string user_name = _userService.GetName();
            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT uid FROM Users WHERE name = @name";


            throw new NotImplementedException();
        }

        /// <summary>
        /// Update an order
        /// </summary>
        /// <param name="uid">ID of an order</param>
        /// <param name="request">Order JSON</param>
        /// <returns>Modified order</returns>
        [HttpPut("orders/{id}")]
        [Authorize]
        public ActionResult<RowerReturnable> UpdateOrder(long uid, RowerDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set current order status
        /// </summary>
        /// <param name="uid">ID of an order</param>
        /// <param name="request">JSON with order status</param>
        /// <returns>Modified order</returns>
        [HttpPut("order/{id}/status")]
        [Authorize]
        public ActionResult<RowerReturnable> UpdateOrderStatus(long uid, RowerStatusDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();

            if (request.Status == "Zrealizowane" && permission != "shop") return Unauthorized("You don't have permission to change status to 'Zrealizowane'");
            if (request.Status == "Oczekujące" && permission != "service") return Unauthorized("You don't have permission to change status to 'Oczekujące'");
            if (request.Status == "W trakcie realizacji" && permission != "service") return Unauthorized("You don't have permission to change status to 'W trakcie realizacji'");

            List<string> validStatuses = new() { "Zrealizowane", "Oczekujące", "W trakcie realizacji", "Nowe zamówienie" };
            if (!validStatuses.Contains(request.Status)) return BadRequest("Invalid status");


            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all user orders
        /// </summary>
        /// <returns>List of user orders</returns>
        [HttpGet("/my-orders")]
        [Authorize(Roles = "Customer")]
        public ActionResult<IEnumerable<RowerReturnable>> GetMyOrders() {
            string _name = _userService.GetName();
            throw new NotImplementedException();
        }

    }
}
