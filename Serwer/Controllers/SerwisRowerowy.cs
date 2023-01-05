using Microsoft.AspNetCore.Mvc;

namespace Serwer.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class SerwisRowerowy : ControllerBase {
        
        [HttpGet(Name = "GetTest")]
        public string GetTest() {
            return DateTime.Now.ToString();
        }

    }
}
