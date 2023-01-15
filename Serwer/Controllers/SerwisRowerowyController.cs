using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Serwer.Controllers {

    [Route("api/service")]
    [ApiController]
    public class SerwisRowerowyController : ControllerBase {
        private static readonly SqliteConnection _connection = new("Data Source=database/serwis.sqlite");

        public SerwisRowerowyController() {
            _connection.Open();
        }

        ~SerwisRowerowyController() {
            _connection.Close();
        }

        [HttpGet("GetTest")]
        public string GetTest() {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT random()";
            var result = command.ExecuteScalar();
            var resultString = result?.ToString();
            return resultString ?? "null";
        }

    }
}
