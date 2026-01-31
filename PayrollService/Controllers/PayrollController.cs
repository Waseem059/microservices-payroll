using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayrollService.Data;
using PayrollService.Models;
using PayrollService.Services;
using System.ComponentModel.Design;

namespace PayrollService.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
        {
        private readonly PayrollContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IntegrationServiceClient _integrationService;
        public PayrollController(PayrollContext context,IMemoryCache memoryCache, IntegrationServiceClient integrationService)
            {
            _context = context;
            _memoryCache = memoryCache;
            _integrationService = integrationService;
            }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
            {
            const string cacheKey = "AllEmployees";
            if (_memoryCache.TryGetValue(cacheKey, out List<Employee> cachedEmployees))
                {
                return Ok(cachedEmployees);
                }
            var employees = await _context.Employees.ToListAsync();
            // Store in cache (expires in 10 minutes)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            _memoryCache.Set(cacheKey, employees, cacheOptions);

            return Ok(employees);
            }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculatePayroll([FromBody] PayrollCalculation request)
            {

            string cacheKey = $"PayrollCalc_{request.EmployeeId}_{request.BaseSalary}";

            // Try to get from cache
            if (_memoryCache.TryGetValue(cacheKey, out object cachedResult))
                {
                return Ok(cachedResult);
                }

            // Cache miss - query database
            var employee = await _context.Employees.FindAsync(request.EmployeeId);
            if (employee == null)
                return NotFound("Employee not found");

            var result = new
                {

                grossSalary = request.BaseSalary,
                tax = employee.BaseSalary * 0.10m,
                netSalary = employee.BaseSalary - (employee.BaseSalary * 0.10m),
                message = "Payroll calculated successfully"

                };

            // Cache for 1 hour (payroll doesn't change often)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            _memoryCache.Set(cacheKey, result, cacheOptions);

            return Ok(result);
            }

            [HttpPost("process")]
        public async Task< IActionResult> ProcessPayroll([FromBody] PayrollCalculation request)
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
            var syncResult = await _integrationService.SyncPayrollWithGusto(request.EmployeeId.ToString());
            return Ok(new { message = "Payroll processed", record });
            }
        }
    }
