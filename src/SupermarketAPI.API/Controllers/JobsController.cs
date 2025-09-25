using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SupermarketAPI.Scrapers.Services;
using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.API.Services;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly ScraperOrchestrator _orchestrator;
        private readonly IRankingService _rankingService;
        private readonly INotificationService _notificationService;

        public JobsController(ScraperOrchestrator orchestrator, IRankingService rankingService, INotificationService notificationService)
        {
            _orchestrator = orchestrator;
            _rankingService = rankingService;
            _notificationService = notificationService;
        }

        [HttpPost("scrape-now")]
        public IActionResult ScrapeNow()
        {
            BackgroundJob.Enqueue(() => _orchestrator.ExecuteScrapingAsync());
            return Accepted();
        }

        [HttpPost("schedule-daily")] 
        public IActionResult ScheduleDaily()
        {
            RecurringJob.AddOrUpdate("daily-scraping", () => _orchestrator.ExecuteScrapingAsync(), Cron.Daily(3));
            RecurringJob.AddOrUpdate("daily-rankings", () => _rankingService.GenerateAllRankingsAsync(), Cron.Daily(4));
            RecurringJob.AddOrUpdate("daily-notifications", () => _notificationService.ProcessDailyNotificationsAsync(), Cron.Daily(7));
            RecurringJob.AddOrUpdate("price-drop-alerts", () => _notificationService.ProcessPriceDropAlertsAsync(), Cron.Hourly(2));
            RecurringJob.AddOrUpdate<DataCleanupService>("weekly-cleanup", s => s.RunWeeklyCleanupAsync(), Cron.Weekly(DayOfWeek.Sunday, 2));
            RecurringJob.AddOrUpdate<AnalyticsUpdateService>("analytics-update", s => s.UpdateMetricsAsync(), Cron.Daily(5));
            return Ok();
        }
    }
}


