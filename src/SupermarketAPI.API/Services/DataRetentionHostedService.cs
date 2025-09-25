using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Services
{
    public class DataRetentionHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataRetentionHostedService> _logger;

        public DataRetentionHostedService(IServiceProvider serviceProvider, ILogger<DataRetentionHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Executa imediatamente na inicialização e depois a cada 3 dias
            await RunOnceAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromDays(3), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                await RunOnceAsync(stoppingToken);
            }
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupermarketDbContext>();

            try
            {
                _logger.LogInformation("[DataRetention] Iniciando limpeza de dados antigos...");

                // Determina a última data de extração existente
                var lastExtractionDate = await db.ProductPrices
                    .OrderByDescending(pp => pp.ScrapedAt)
                    .Select(pp => pp.ScrapedAt.Date)
                    .FirstOrDefaultAsync(ct);

                if (lastExtractionDate == default)
                {
                    _logger.LogInformation("[DataRetention] Nenhum dado encontrado para retenção.");
                    return;
                }

                var keepFrom = lastExtractionDate; // manter somente o último dia de extração

                // Remove ProductPrices com ScrapedAt.Date < keepFrom
                var oldPrices = db.ProductPrices.Where(pp => pp.ScrapedAt.Date < keepFrom);
                db.ProductPrices.RemoveRange(oldPrices);

                // Limpa logs antigos, rankings, etc. anteriores a keepFrom
                var oldLogs = db.ScrapingLogs.Where(l => l.StartedAt.Date < keepFrom);
                db.ScrapingLogs.RemoveRange(oldLogs);

                var oldRankings = db.DailyRankings.Where(r => r.Date.Date < keepFrom);
                db.DailyRankings.RemoveRange(oldRankings);

                var oldNotifications = db.NotificationLogs.Where(n => n.SentAt.Date < keepFrom);
                db.NotificationLogs.RemoveRange(oldNotifications);

                await db.SaveChangesAsync(ct);

                _logger.LogInformation("[DataRetention] Limpeza concluída. Mantido apenas o último dia de extração: {Date}", keepFrom);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DataRetention] Erro durante a limpeza de retenção de dados");
            }
        }
    }
}


