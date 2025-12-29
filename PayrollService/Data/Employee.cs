namespace PayrollService.Data
    {
    public class Employee
        {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal BaseSalary { get; set; }
        }

    public class PayrollRecord
        {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Tax { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime PayDate { get; set; }
        }
    }