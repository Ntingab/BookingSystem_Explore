using Microsoft.AspNetCore.Mvc;
using BookingSystem_Explore.Infrastructure.Constants;
using Asp.Versioning;

namespace BookingSystem_Explore.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion(Versions.Version1)]
    public class TestController : ControllerBase
    {
        [HttpGet("throw")]
        public IActionResult Throw() => throw new System.Exception("Test exception from TestController");
    }
}
