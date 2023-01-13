using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Serwer.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class SerwisRowerowy : ControllerBase {
        private static readonly SqliteConnection _connection = new("Data Source=database/serwis.sqlite");

        public SerwisRowerowy() {
            _connection.Open();
        }

        ~SerwisRowerowy() {
            _connection.Close();
        }

        [HttpGet(Name = "GetTest")]
        public string GetTest() {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT random()";
            var result = command.ExecuteScalar();
            var resultString = result?.ToString();
            return resultString ?? "null";
        }
        
        [HttpPost]
        [Produces("application/json")]
        public dynamic Post([FromForm] string? value, [FromForm] string? v2) {
            dynamic _dynamic = new System.Dynamic.ExpandoObject();
            dynamic dyn = new System.Dynamic.ExpandoObject();
            dyn.a = "A";
            dyn.b = "B";
            dyn.c = "C";
            dyn.d = v2;
            _dynamic.value = value;
            _dynamic.dynamic = dyn;
            _dynamic.time = DateTime.Now.ToString();
            return _dynamic;
        }

    }
}
