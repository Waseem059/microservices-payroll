using Microsoft.AspNetCore.Mvc;
using IntegrationService.Services;

namespace IntegrationService.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
        {
        private readonly GustoMockService _gustoService;

        public IntegrationController(GustoMockService gustoService)
            {
            _gustoService = gustoService;
            }

        [HttpPost("sync-gusto")]
        public IActionResult SyncGusto([FromBody] SyncRequest request)
            {
            if (string.IsNullOrEmpty(request.CompanyId))
                return BadRequest("Company ID required");

            var result = _gustoService.SyncPayroll(request.CompanyId);
            return Ok(result);
            }

        [HttpGet("gusto-status/{companyId}")]
        public IActionResult GetGustoStatus(string companyId)
            {
            var status = _gustoService.GetSyncStatus(companyId);
            return Ok(status);
            }
        }

    public class SyncRequest
        {
        public string CompanyId { get; set; }
        }
    }
