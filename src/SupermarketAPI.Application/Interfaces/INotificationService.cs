using SupermarketAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendDailyFavoritesNotificationAsync(int userId);
        Task<bool> SendPriceDropAlertAsync(int userId, int productId, decimal oldPrice, decimal newPrice);
        Task<bool> SendWeeklySummaryAsync(int userId);
        Task<bool> SendWhatsAppMessageAsync(string phoneNumber, string message);
        Task<bool> ConfigureNotificationSettingsAsync(int userId, UserNotificationSettingsDto settings);
        Task<IEnumerable<NotificationLogDto>> GetUserNotificationHistoryAsync(int userId, int skip = 0, int take = 50);
        Task<bool> ProcessDailyNotificationsAsync();
        Task<bool> ProcessPriceDropAlertsAsync();
    }

    public class NotificationLogDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string Channel { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal? TotalSavings { get; set; }
    }
}
