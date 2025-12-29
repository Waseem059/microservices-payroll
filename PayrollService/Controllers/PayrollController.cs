using Microsoft.AspNetCore.Mvc;
using PayrollService.Data;
using PayrollService.Models;

namespace PayrollService.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
        {
        private readonly PayrollContext _context;

        public PayrollController(PayrollContext context)
            {
            _context = context;
            }

        [HttpGet("employees")]
        public IActionResult GetEmployees()
            {
            var employees = _context.Employees.ToList();
            return Ok(employees);
            }

        [HttpPost("calculate")]
        public IActionResult CalculatePayroll([FromBody] PayrollCalculation request)
            {
            if (request.BaseSalary <= 0)
                return BadRequest("Invalid salary");

            // Simple tax calculation (10%)
            decimal tax = request.BaseSalary * 0.10m;
            decimal netSalary = request.BaseSalary - tax;

            return Ok(new
                {
                grossSalary = request.BaseSalary,
                tax = tax,
                netSalary = netSalary,
                message = "Payroll calculated successfully"
                });
            }

        [HttpPost("process")]
        public IActionResult ProcessPayroll([FromBody] PayrollCalculation request)
            {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == request.EmployeeId);
            if (employee == null)
                return NotFound("Employee not found");

            decimal tax = employee.BaseSalary * 0.10m;
            decimal netSalary = employee.BaseSalary - tax;

            var record = new PayrollRecord
                {
                EmployeeId = employee.Id,
                GrossSalary = employee.BaseSalary,
                Tax = tax,
                NetSalary = netSalary,
                PayDate = DateTime.UtcNow
                };

            _context.PayrollRecords.Add(record);
            _context.SaveChanges();

            return Ok(new { message = "Payroll processed", record });
            }
        }
    }
