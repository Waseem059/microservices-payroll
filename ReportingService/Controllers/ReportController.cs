using Microsoft.AspNetCore.Mvc;
using ReportingService.Services;

namespace ReportingService.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
        {
        private readonly ReportGenerator _reportGenerator;

        public ReportController(ReportGenerator reportGenerator)
            {
            _reportGenerator = reportGenerator;
            }

        [HttpPost("payroll-summary")]
        public IActionResult GeneratePayrollSummary([FromBody] ReportRequest request)
            {
            if (request.Month <= 0 || request.Year <= 0)
                return BadRequest("Valid month and year required");

            var report = _reportGenerator.GeneratePayrollSummary(request.Month, request.Year);
            return Ok(report);
            }

        [HttpGet("employee-report/{employeeId}")]
        public IActionResult GetEmployeeReport(int employeeId)
            {
            var report = _reportGenerator.GenerateEmployeeReport(employeeId);
            return Ok(report);
            }
        }

    public class ReportRequest
        {
        public int Month { get; set; }
        public int Year { get; set; }
        }
    }
