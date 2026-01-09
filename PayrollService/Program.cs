// PayrollService/Program.cs - CORRECTED VERSION
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PayrollService.Data;
var connectionString = "Data Source=KFWWASNAB\\SQL2022;Initial Catalog=PayrollDB;User Id=sa;Password=bqeko@123;Pooling=true;Max Pool Size=100;Min Pool Size=10;Connection Lifetime=1800";
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<PayrollContext>(options =>
//options.UseSqlServer("Server=host.docker.internal;Database=PayrollDB;User Id=sa;Password=bqeko@123;TrustServerCertificate=True;")

  options.UseSqlServer("Data Source=KFWWASNAB\\SQL2022;Initial Catalog=PayrollDB;User Id=sa;Password=bqeko@123; MultipleActiveResultSets=true;Enlist=true;TrustServerCertificate=True;Pooling=true;Max Pool Size=100;Min Pool Size=10;Connection Lifetime=1800")

);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Create database on startup
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<PayrollContext>();

    // Retry 5 times with 5 second delay
    int retries = 0;
    while (retries < 5)
        {
        try
            {
            db.Database.EnsureCreated();
            break;
            }
        catch (Exception ex)
            {
            retries++;
            if (retries >= 5)
                throw;
            System.Threading.Thread.Sleep(5000); // Wait 5 seconds before retry
            }
        }
    }

if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5002");