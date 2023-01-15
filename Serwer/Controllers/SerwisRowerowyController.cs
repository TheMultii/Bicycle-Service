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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get bike by ID
        /// </summary>
        /// <param name="id">ID of bike</param>
        /// <returns>Bike if exists</returns>
        [HttpGet("bicycles/{id}")]
        [Authorize]
        public ActionResult<Rower> GetBicycle(long id) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        /// <returns>List of orders</returns>
        [HttpGet("orders")]
        [Authorize]
        public ActionResult<IEnumerable<Zamówienie>> GetOrders() {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();
            List<Zamówienie> _orders = new();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get order by id
        /// </summary>
        /// <param name="id">ID of order</param>
        /// <returns>Order if exists</returns>
        [HttpGet("orders/{id}")]
        [Authorize]
        public ActionResult<Zamówienie> GetOrder(long id) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();
            //Zamówienie _order = new();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="request">User and bike</param>
        /// <returns>Confirmation of order</returns>
        [HttpPost("orders")]
        [Authorize]
        public ActionResult<Zamówienie> CreateOrder(ZamówienieDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update an order
        /// </summary>
        /// <param name="id">ID of an order</param>
        /// <param name="request">Order JSON</param>
        /// <returns>Modified order</returns>
        [HttpPut("orders/{id}")]
        [Authorize]
        public ActionResult<Zamówienie> UpdateOrder(long id, ZamówienieDTO request) {
            string permission = _userService.GetRole().ToLower();
            if (permission != "service" || permission != "shop") return Unauthorized();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set current order status
        /// </summary>
        /// <param name="id">ID of an order</param>
        /// <param name="request">JSON with order status</param>
        /// <returns>Modified order</returns>
        [HttpPut("order/{id}/status")]
        [Authorize]
        public ActionResult<Zamówienie> UpdateOrderStatus(long id, ZamówienieStatusDTO request) {
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
        public ActionResult<IEnumerable<Zamówienie>> GetMyOrders() {
            string _name = _userService.GetName();
            throw new NotImplementedException();
        }

    }
}
