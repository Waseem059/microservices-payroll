using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for calling other services
builder.Services.AddHttpClient("AuthService", client =>
    client.BaseAddress = new Uri("http://localhost:5001"));
builder.Services.AddHttpClient("PayrollService", client =>
    client.BaseAddress = new Uri("http://localhost:5002"));
builder.Services.AddHttpClient("IntegrationService", client =>
    client.BaseAddress = new Uri("http://localhost:5003"));
builder.Services.AddHttpClient("ReportingService", client =>
    client.BaseAddress = new Uri("http://localhost:5004"));

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

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5000");