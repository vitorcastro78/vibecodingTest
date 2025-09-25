using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Services
{
    public class DataCleanupService
    {
        private readonly SupermarketDbContext _db;
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(SupermarketDbContext db, ILogger<DataCleanupService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task RunWeeklyCleanupAsync()
        {
            try
            {
                _logger.LogInformation("[Cleanup] Iniciando limpeza semanal...");

                var threshold = DateTime.UtcNow.Date.AddDays(-30);

                var oldLogs = _db.NotificationLogs.Where(n => n.SentAt < threshold);
                _db.NotificationLogs.RemoveRange(oldLogs);

                var oldScrapings = _db.ScrapingLogs.Where(s => s.StartedAt < threshold);
                _db.ScrapingLogs.RemoveRange(oldScrapings);

                await _db.SaveChangesAsync();

                _logger.LogInformation("[Cleanup] Limpeza semanal concluída.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cleanup] Erro durante limpeza semanal");
            }
        }
    }
}


