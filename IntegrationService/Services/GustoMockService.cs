namespace IntegrationService.Services
    {
    public class GustoMockService
        {
        public object SyncPayroll(string companyId)
            {
            // Mock Gusto API call
            return new
                {
                companyId = companyId,
                status = "success",
                syncedRecords = 150,
                timestamp = DateTime.UtcNow,
                message = "Payroll synced with Gusto successfully"
                };
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
