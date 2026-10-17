using Microsoft.AspNetCore.Mvc;

namespace BookingSystem_Explore.Api.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        [HttpGet("throw")]
        public IActionResult Throw() => throw new System.Exception("Test exception from TestController");
    }
}
