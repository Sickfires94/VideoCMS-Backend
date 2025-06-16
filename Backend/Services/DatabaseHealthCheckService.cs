using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Backend.Services
{

    public class DatabaseHealthCheckService
    {
        private readonly VideoManagementApplicationContext _dbContext;

        public DatabaseHealthCheckService(VideoManagementApplicationContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsDatabaseConnectedAsync()
        {
            try
            {
                return await _dbContext.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"Database connection failed: {ex.Message}");
                return false;
            }
        }
    }
}
