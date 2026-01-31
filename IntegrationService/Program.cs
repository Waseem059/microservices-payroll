using IntegrationService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    // Check 1: Gusto API availability (simulated with httpstat.us)
    .AddUrlGroup(
        new Uri("https://httpstat.us/200"),
        name: "gusto_api",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "external", "gusto" },
        timeout: TimeSpan.FromSeconds(5)
    )
    // Check 2: Self memory check
    .AddCheck("self_memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var threshold = 1024L * 1024L * 500L; // 500 MB

        if (allocated < threshold)
        {
            return HealthCheckResult.Healthy($"Memory: {allocated / 1024 / 1024} MB");
        }
        return HealthCheckResult.Degraded($"High memory: {allocated / 1024 / 1024} MB");
    });

// ===== RETRY POLICY =====
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,  // Retry up to 3 times
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"🔄 RETRY #{retryCount}: Waiting {timespan.TotalSeconds}s before retry...");
            Console.WriteLine($"   Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
        }
    );

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Handles 5xx and 408 errors
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,  // Open circuit after 3 failures
        durationOfBreak: TimeSpan.FromSeconds(30), // Keep circuit open for 30 seconds
        onBreak: (outcome, timespan) =>
        {
            Console.WriteLine($"🔴 CIRCUIT BREAKER OPENED! Will retry after {timespan.TotalSeconds} seconds");
            Console.WriteLine($"   Reason: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
        },
        onReset: () =>
        {
            Console.WriteLine($"🟢 CIRCUIT BREAKER CLOSED! Back to normal operation");
        },
        onHalfOpen: () =>
        {
            Console.WriteLine($"🟡 CIRCUIT BREAKER HALF-OPEN! Testing if service recovered...");
        }
    );

//builder.Services.AddSingleton<GustoMockService>();
builder.Services.AddHttpClient<GustoMockService>().AddPolicyHandler(retryPolicy).AddPolicyHandler(circuitBreakerPolicy);
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

if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
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
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready");
app.Run("http://localhost:5003");