using Microsoft.EntityFrameworkCore;

namespace PayrollService.Data
    {
    public class PayrollContext : DbContext
        {
        public PayrollContext(DbContextOptions<PayrollContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<PayrollRecord> PayrollRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            base.OnModelCreating(modelBuilder);

            // Seed sample data
            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, Name = "John Doe", BaseSalary = 50000 },
                new Employee { Id = 2, Name = "Jane Smith", BaseSalary = 60000 }
            );
            }
        }
    }