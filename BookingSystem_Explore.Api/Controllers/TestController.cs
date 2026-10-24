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
        private readonly IHostEnvironment _environment;

        public TestController(IHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet("throw")]
        public IActionResult Throw()
        {
            if (!_environment.IsDevelopment() && !_environment.IsEnvironment("IntegrationTests")) return NotFound();
            throw new System.Exception("Test exception from TestController");
        }
    }
}
