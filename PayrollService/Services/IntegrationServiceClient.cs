using System.Text.Json;
using Polly.CircuitBreaker;

namespace PayrollService.Services
{
    public class IntegrationServiceClient
    {
        private readonly HttpClient _httpClient;

        public IntegrationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<object> SyncPayrollWithGusto(string companyId)
        {
            try
            {
                Console.WriteLine($"\n[PayrollService] → Calling IntegrationService to sync company {companyId}");

                var response = await _httpClient.PostAsync($"/api/Gusto/sync/{companyId}", null);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<object>(content);

                Console.WriteLine($"[PayrollService] ✅ IntegrationService responded successfully");

                return result;
            }
            catch (BrokenCircuitException ex)
            {
                Console.WriteLine($"[PayrollService] ⚡ Circuit OPEN - IntegrationService unavailable");

                // Return fallback response
                return new
                {
                    companyId = companyId,
                    status = "fallback",
                    message = "Integration service temporarily unavailable. Payroll calculated but not synced to Gusto.",
                    timestamp = DateTime.UtcNow,
                    circuitOpen = true
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[PayrollService] ❌ Failed to call IntegrationService: {ex.Message}");
                throw;
            }
        }
    }
}