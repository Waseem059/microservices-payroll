namespace ReportingService.Services
    {
    public class ReportGenerator
        {
        public object GeneratePayrollSummary(int month, int year)
            {
            return new
                {
                month = month,
                year = year,
                totalEmployees = 2,
                totalGrossSalary = 110000,
                totalTax = 11000,
                totalNetSalary = 99000,
                generatedAt = DateTime.UtcNow,
                message = "Payroll summary report generated"
                };
            }

        public object GenerateEmployeeReport(int employeeId)
            {
            return new
                {
                employeeId = employeeId,
                employeeName = $"Employee {employeeId}",
                totalPayrollRecords = 12,
                ytdGrossSalary = 600000,
                ytdTax = 60000,
                ytdNetSalary = 540000,
                generatedAt = DateTime.UtcNow
                };
            }
        }
    }
