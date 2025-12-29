using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
        {
        [HttpGet]
        public IActionResult GetHealth()
            {
            return Ok(new { status = "All services running" });
            }
        }
    }