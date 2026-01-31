using Polly.CircuitBreaker;

namespace IntegrationService.Services
    {
    public class GustoMockService
        {
        private readonly HttpClient _httpClient;
        private static int _callCounter = 0;  // Track number of calls for demo purposes

        public GustoMockService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://httpstat.us/"); // Mock HTTP status service
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<object> SyncPayroll(string companyId)
        {
            try
            {
                _callCounter++;
                Console.WriteLine($"\n📞 Call #{_callCounter}: Attempting to sync payroll for company {companyId}...");

                // Simulate calling Gusto API
                // For demo: First 3 calls fail (500 error), then succeed (200)
                string endpoint = _callCounter <= 3 ? "500" : "200";  // Fail first 3, then succeed

                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                Console.WriteLine($"✅ Call #{_callCounter}: SUCCESS - Payroll synced!");

                return new
                {
                    companyId = companyId,
                    status = "success",
                    syncedRecords = 150,
                    timestamp = DateTime.UtcNow,
                    message = "Payroll synced with Gusto successfully",
                    callNumber = _callCounter
                };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Call #{_callCounter}: FAILED - {ex.Message}");
                throw; // Circuit Breaker will catch this
            }
            catch (BrokenCircuitException ex)
            {
                Console.WriteLine($"⚡ Call #{_callCounter}: CIRCUIT OPEN - Request blocked!");

                // Return fallback response when circuit is open
                return new
                {
                    companyId = companyId,
                    status = "circuit_open",
                    message = "Gusto API temporarily unavailable. Using cached data.",
                    timestamp = DateTime.UtcNow,
                    callNumber = _callCounter,
                    fallback = true
                };
            }
        }

        public object GetSyncStatus(string companyId)
            {
            return new
                {
                companyId = companyId,
                lastSync = DateTime.UtcNow.AddHours(-2),
                status = "synced",
                recordsCount = 150
                };
            }
        }
    }
