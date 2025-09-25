using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SupermarketAPI.Infrastructure.Data;
using SupermarketAPI.Notifications.Services;
using SupermarketAPI.Domain.Entities;
using Xunit;

namespace SupermarketAPI.Tests.Notifications.Services
{
    public class WhatsAppNotificationServiceTests : IDisposable
    {
        private readonly SupermarketDbContext _context;
        private readonly Mock<ILogger<WhatsAppNotificationService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly WhatsAppNotificationService _service;

        public WhatsAppNotificationServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<SupermarketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SupermarketDbContext(options);

            // Setup mocks
            _loggerMock = new Mock<ILogger<WhatsAppNotificationService>>();
            _configurationMock = new Mock<IConfiguration>();

            // Setup configuration
            _configurationMock.Setup(c => c["Twilio:AccountSid"]).Returns("");
            _configurationMock.Setup(c => c["Twilio:AuthToken"]).Returns("");
            _configurationMock.Setup(c => c["Twilio:WhatsAppNumber"]).Returns("+14155238886");

            _service = new WhatsAppNotificationService(_context, _loggerMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task SendDailyFavoritesNotificationAsync_ShouldReturnTrueForValidUser()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            await CreateTestFavoritesAsync(user.Id);

            // Act
            var result = await _service.SendDailyFavoritesNotificationAsync(user.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendDailyFavoritesNotificationAsync_ShouldReturnFalseForUserWithoutWhatsApp()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            user.WhatsAppNumber = null;
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.SendDailyFavoritesNotificationAsync(user.Id);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendDailyFavoritesNotificationAsync_ShouldReturnFalseForNonExistentUser()
        {
            // Arrange
            var nonExistentUserId = 999;

            // Act
            var result = await _service.SendDailyFavoritesNotificationAsync(nonExistentUserId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendPriceDropAlertAsync_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;
            var productId = 1;
            var oldPrice = 2.50m;
            var newPrice = 2.00m;

            // Act
            var result = await _service.SendPriceDropAlertAsync(userId, productId, oldPrice, newPrice);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendWeeklySummaryAsync_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;

            // Act
            var result = await _service.SendWeeklySummaryAsync(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendWhatsAppMessageAsync_ShouldLogMessageWhenTwilioNotConfigured()
        {
            // Arrange
            var phoneNumber = "+351912345678";
            var message = "Test message";

            // Act
            var result = await _service.SendWhatsAppMessageAsync(phoneNumber, message);

            // Assert
            result.Should().BeTrue();
            
            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Twilio não configurado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ConfigureNotificationSettingsAsync_ShouldCreateNewSettings()
        {
            // Arrange
            var userId = 1;
            var settings = new SupermarketAPI.Application.DTOs.UserNotificationSettingsDto
            {
                WhatsAppEnabled = true,
                NotificationDays = "Monday,Wednesday,Friday",
                PriceDropThreshold = 0.1m,
                MaxNotificationsPerDay = 5
            };

            // Act
            var result = await _service.ConfigureNotificationSettingsAsync(userId, settings);

            // Assert
            result.Should().BeTrue();
            
            var savedSettings = await _context.UserNotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);
            savedSettings.Should().NotBeNull();
            savedSettings!.WhatsAppEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserNotificationHistoryAsync_ShouldReturnEmptyListForNewUser()
        {
            // Arrange
            var userId = 1;

            // Act
            var history = await _service.GetUserNotificationHistoryAsync(userId);

            // Assert
            history.Should().NotBeNull();
            history.Should().BeEmpty();
        }

        [Fact]
        public async Task ProcessDailyNotificationsAsync_ShouldReturnTrue()
        {
            // Act
            var result = await _service.ProcessDailyNotificationsAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ProcessPriceDropAlertsAsync_ShouldReturnTrue()
        {
            // Act
            var result = await _service.ProcessPriceDropAlertsAsync();

            // Assert
            result.Should().BeTrue();
        }

        private async Task<User> CreateTestUserAsync()
        {
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                WhatsAppNumber = "+351912345678",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private async Task CreateTestFavoritesAsync(int userId)
        {
            var category = new Category
            {
                Id = 1,
                Name = "Laticínios",
                CreatedAt = DateTime.UtcNow
            };

            var product = new Product
            {
                Id = 1,
                Name = "Leite UHT 1L",
                CategoryId = category.Id,
                Category = category,
                AveragePrice = 1.20m,
                CreatedAt = DateTime.UtcNow
            };

            var favorite = new UserFavorite
            {
                Id = 1,
                UserId = userId,
                ProductId = product.Id,
                Product = product,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            _context.Products.Add(product);
            _context.UserFavorites.Add(favorite);
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
