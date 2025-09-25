using System;
using System.Collections.Generic;

namespace SupermarketAPI.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsWhatsAppVerified { get; set; }
        public UserNotificationSettingsDto? NotificationSettings { get; set; }
        public List<UserFavoriteDto> Favorites { get; set; } = new List<UserFavoriteDto>();
    }

    public class UserFavoriteDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public decimal? PriceThreshold { get; set; }
        public bool IsPriceAlertEnabled { get; set; }
        public DateTime AddedAt { get; set; }
        public ProductPriceDto? BestPrice { get; set; }
    }

    public class UserNotificationSettingsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool WhatsAppEnabled { get; set; }
        public bool EmailEnabled { get; set; }
        public TimeSpan? NotificationTime { get; set; }
        public string NotificationDays { get; set; } = string.Empty;
        public decimal PriceDropThreshold { get; set; }
        public int MaxNotificationsPerDay { get; set; }
        public bool DailyFavoritesEnabled { get; set; }
        public bool PriceDropAlertsEnabled { get; set; }
        public bool WeeklySummaryEnabled { get; set; }
        public DateTime? LastNotificationSent { get; set; }
        public int NotificationsSentToday { get; set; }
    }
}
