using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Infrastructure.Data;
using System.Security.Claims;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly SupermarketDbContext _db;

        public UsersController(SupermarketDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var favorites = await _db.UserFavorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                .ThenInclude(p => p.ProductPrices)
                .Select(f => new UserFavoriteDto
                {
                    Id = f.Id,
                    ProductId = f.ProductId,
                    ProductName = f.Product.Name,
                    ProductImageUrl = f.Product.ImageUrl,
                    PriceThreshold = f.PriceThreshold,
                    IsPriceAlertEnabled = f.IsPriceAlertEnabled,
                    AddedAt = f.AddedAt
                })
                .ToListAsync();
            return Ok(favorites);
        }

        [Authorize]
        [HttpPost("favorites")]
        public async Task<IActionResult> AddFavorite([FromBody] int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var exists = await _db.UserFavorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId);
            if (exists) return Conflict("Já é favorito");

            _db.UserFavorites.Add(new Domain.Entities.UserFavorite
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow,
                IsActive = true
            });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpDelete("favorites/{productId:int}")]
        public async Task<IActionResult> RemoveFavorite([FromRoute] int productId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var fav = await _db.UserFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
            if (fav == null) return NotFound();
            _db.UserFavorites.Remove(fav);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        public record WhatsAppConfigRequest(string Number);

        [Authorize]
        [HttpPost("whatsapp")]
        public async Task<IActionResult> ConfigureWhatsApp([FromBody] WhatsAppConfigRequest req)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();
            user.WhatsAppNumber = req.Number;
            await _db.SaveChangesAsync();
            return Ok();
        }

        public record NotificationPrefsRequest(bool WhatsAppEnabled, string NotificationDays, int MaxNotificationsPerDay, decimal PriceDropThreshold);

        [Authorize]
        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotificationPrefs([FromBody] NotificationPrefsRequest req)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var settings = await _db.UserNotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
            {
                settings = new Domain.Entities.UserNotificationSettings
                {
                    UserId = userId,
                    WhatsAppEnabled = req.WhatsAppEnabled,
                    NotificationDays = req.NotificationDays,
                    MaxNotificationsPerDay = req.MaxNotificationsPerDay,
                    PriceDropThreshold = req.PriceDropThreshold,
                    IsActive = true
                };
                _db.UserNotificationSettings.Add(settings);
            }
            else
            {
                settings.WhatsAppEnabled = req.WhatsAppEnabled;
                settings.NotificationDays = req.NotificationDays;
                settings.MaxNotificationsPerDay = req.MaxNotificationsPerDay;
                settings.PriceDropThreshold = req.PriceDropThreshold;
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}


