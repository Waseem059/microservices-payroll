// PayrollService/Program.cs - CORRECTED VERSION
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PayrollService.Data;
using Polly.Extensions.Http;
using PayrollService.Services;
using Polly;
using Microsoft.Extensions.Diagnostics.HealthChecks;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
string connectionString = "Data Source=DESKTOP-F40620V\\SQLEXPRESS;Initial Catalog=PayrollDB;User Id=sa;Password=iaf349; MultipleActiveResultSets=true;Enlist=true;TrustServerCertificate=True;Pooling=true;Max Pool Size=100;Min Pool Size=10;Connection Lifetime=1800";
// Add DbContext
builder.Services.AddDbContext<PayrollContext>(options =>
//options.UseSqlServer("Server=host.docker.internal;Database=PayrollDB;User Id=sa;Password=bqeko@123;TrustServerCertificate=True;")

  options.UseSqlServer(connectionString)

);

builder.Services.AddHealthChecks().AddSqlServer(
    connectionString:connectionString,name: "PayrollDB", 
    failureStatus: HealthStatus.Unhealthy, tags: new[] { "database","sql"}
    )
    .AddUrlGroup(
    new Uri("http://localhost:5003/health"),
        name: "integration_service",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "service", "integration" },
        timeout: TimeSpan.FromSeconds(5)
    ) .AddCheck("self_memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var threshold = 1024L * 1024L * 500L; // 500 MB threshold

        if (allocated < threshold)
        {
            return HealthCheckResult.Healthy($"Memory usage: {allocated / 1024 / 1024} MB");
        }
        return HealthCheckResult.Degraded($"High memory usage: {allocated / 1024 / 1024} MB");
    }); 
IAsyncPolicy<HttpResponseMessage> integrationServiceCircuitBreaker = HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync
    (
    handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(30),onBreak
    : (Outcome, timespan) => {
        Console.WriteLine($"🔴 [PayrollService] Circuit to IntegrationService OPENED!");
        Console.WriteLine($"   Will retry after {timespan.TotalSeconds}s");

    }, onReset: () =>
    {
        Console.WriteLine($"🟢 [PayrollService] Circuit to IntegrationService CLOSED!");
    },
     onHalfOpen: () =>
     {
         Console.WriteLine($"🟡 [PayrollService] Circuit to IntegrationService HALF-OPEN!");
     }
    );
builder.Services.AddHttpClient<IntegrationServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5003"); // IntegrationService URL
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddPolicyHandler(integrationServiceCircuitBreaker);
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


// ===== HEALTH CHECK ENDPOINTS =====
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Simple liveness check (is service running?)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // No checks, just respond if alive
});

// Readiness check (ready for traffic?)
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});


app.Run("http://localhost:5002");