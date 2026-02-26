using Microsoft.AspNetCore.Mvc;

namespace DirtyApi.Controllers
{
    public class TestController : ControllerBase
    {
        [HttpGet("/api/v1/test")]
        public IActionResult Get()
        {
            return Ok("test");
        }
    }
}
