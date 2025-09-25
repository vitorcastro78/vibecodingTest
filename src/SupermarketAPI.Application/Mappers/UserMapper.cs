using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Domain.Entities;
using System.Linq;

namespace SupermarketAPI.Application.Mappers
{
    public static class UserMapper
    {
        public static UserDto ToDto(User user)
        {
            if (user == null) return null!;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                WhatsAppNumber = user.WhatsAppNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsEmailVerified = user.IsEmailVerified,
                IsWhatsAppVerified = user.IsWhatsAppVerified,
                NotificationSettings = user.NotificationSettings != null ? UserNotificationSettingsMapper.ToDto(user.NotificationSettings) : null,
                Favorites = user.UserFavorites?.Select(uf => UserFavoriteMapper.ToDto(uf)).ToList() ?? new List<UserFavoriteDto>()
            };
        }

        public static User ToEntity(UserDto dto)
        {
            if (dto == null) return null!;

            return new User
            {
                Id = dto.Id,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                WhatsAppNumber = dto.WhatsAppNumber,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = dto.CreatedAt,
                LastLoginAt = dto.LastLoginAt,
                IsEmailVerified = dto.IsEmailVerified,
                IsWhatsAppVerified = dto.IsWhatsAppVerified
            };
        }
    }

    public static class UserFavoriteMapper
    {
        public static UserFavoriteDto ToDto(UserFavorite favorite)
        {
            if (favorite == null) return null!;

            return new UserFavoriteDto
            {
                Id = favorite.Id,
                ProductId = favorite.ProductId,
                ProductName = favorite.Product?.Name ?? string.Empty,
                ProductImageUrl = favorite.Product?.ImageUrl ?? string.Empty,
                PriceThreshold = favorite.PriceThreshold,
                IsPriceAlertEnabled = favorite.IsPriceAlertEnabled,
                AddedAt = favorite.AddedAt,
                BestPrice = favorite.Product?.ProductPrices?.Where(pp => pp.IsAvailable).OrderBy(pp => pp.Price).FirstOrDefault() != null 
                    ? ProductPriceMapper.ToDto(favorite.Product.ProductPrices.Where(pp => pp.IsAvailable).OrderBy(pp => pp.Price).First()) 
                    : null
            };
        }

        public static UserFavorite ToEntity(UserFavoriteDto dto)
        {
            if (dto == null) return null!;

            return new UserFavorite
            {
                Id = dto.Id,
                ProductId = dto.ProductId,
                PriceThreshold = dto.PriceThreshold,
                IsPriceAlertEnabled = dto.IsPriceAlertEnabled,
                AddedAt = dto.AddedAt
            };
        }
    }

    public static class UserNotificationSettingsMapper
    {
        public static UserNotificationSettingsDto ToDto(UserNotificationSettings settings)
        {
            if (settings == null) return null!;

            return new UserNotificationSettingsDto
            {
                Id = settings.Id,
                UserId = settings.UserId,
                WhatsAppEnabled = settings.WhatsAppEnabled,
                EmailEnabled = settings.EmailEnabled,
                NotificationTime = settings.NotificationTime,
                NotificationDays = settings.NotificationDays,
                PriceDropThreshold = settings.PriceDropThreshold,
                MaxNotificationsPerDay = settings.MaxNotificationsPerDay,
                DailyFavoritesEnabled = settings.DailyFavoritesEnabled,
                PriceDropAlertsEnabled = settings.PriceDropAlertsEnabled,
                WeeklySummaryEnabled = settings.WeeklySummaryEnabled,
                LastNotificationSent = settings.LastNotificationSent,
                NotificationsSentToday = settings.NotificationsSentToday
            };
        }

        public static UserNotificationSettings ToEntity(UserNotificationSettingsDto dto)
        {
            if (dto == null) return null!;

            return new UserNotificationSettings
            {
                Id = dto.Id,
                UserId = dto.UserId,
                WhatsAppEnabled = dto.WhatsAppEnabled,
                EmailEnabled = dto.EmailEnabled,
                NotificationTime = dto.NotificationTime,
                NotificationDays = dto.NotificationDays,
                PriceDropThreshold = dto.PriceDropThreshold,
                MaxNotificationsPerDay = dto.MaxNotificationsPerDay,
                DailyFavoritesEnabled = dto.DailyFavoritesEnabled,
                PriceDropAlertsEnabled = dto.PriceDropAlertsEnabled,
                WeeklySummaryEnabled = dto.WeeklySummaryEnabled,
                LastNotificationSent = dto.LastNotificationSent,
                NotificationsSentToday = dto.NotificationsSentToday
            };
        }
    }
}
