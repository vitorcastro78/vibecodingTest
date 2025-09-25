using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Infrastructure.Data;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace SupermarketAPI.Notifications.Services
{
	public class WhatsAppNotificationService : INotificationService
	{
		private readonly SupermarketDbContext _db;
		private readonly ILogger<WhatsAppNotificationService> _logger;
		private readonly IConfiguration _config;
		private readonly string _twilioAccountSid;
		private readonly string _twilioAuthToken;
		private readonly string _twilioWhatsAppNumber;

		public WhatsAppNotificationService(SupermarketDbContext db, ILogger<WhatsAppNotificationService> logger, IConfiguration config)
		{
			_db = db;
			_logger = logger;
			_config = config;
			
			_twilioAccountSid = _config["Twilio:AccountSid"] ?? "";
			_twilioAuthToken = _config["Twilio:AuthToken"] ?? "";
			_twilioWhatsAppNumber = _config["Twilio:WhatsAppNumber"] ?? "";
			
			// Inicializar Twilio
			if (!string.IsNullOrEmpty(_twilioAccountSid) && !string.IsNullOrEmpty(_twilioAuthToken))
			{
				TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
			}
		}

		public async Task<bool> SendDailyFavoritesNotificationAsync(int userId)
		{
			var user = await _db.Users
				.Include(u => u.NotificationSettings)
				.Include(u => u.UserFavorites)
				.ThenInclude(f => f.Product)
				.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null || string.IsNullOrWhiteSpace(user.WhatsAppNumber)) return false;

			var bests = new List<string>();
			foreach (var fav in user.UserFavorites)
			{
				var bestPrice = await _db.ProductPrices
					.Where(pp => pp.ProductId == fav.ProductId && pp.IsAvailable)
					.Include(pp => pp.Supermarket)
					.OrderBy(pp => pp.Price)
					.FirstOrDefaultAsync();
				if (bestPrice != null)
				{
					bests.Add($"{fav.Product.Name} - {bestPrice.Supermarket.Name} €{bestPrice.Price:F2}");
				}
			}

			var today = DateTime.UtcNow.ToString("dd/MM/yyyy");
			var body = $"🛒 *Seus Favoritos Hoje* - {today}\n\n💰 *Melhores Preços:*\n" + string.Join("\n", bests);
			return await SendWhatsAppMessageAsync(user.WhatsAppNumber!, body);
		}

		public Task<bool> SendPriceDropAlertAsync(int userId, int productId, decimal oldPrice, decimal newPrice)
		{
			var msg = $"📉 Queda de preço! Produto {productId} de €{oldPrice:F2} por €{newPrice:F2}";
			// Em produção: buscar telefone do user e enviar
			_logger.LogInformation("[WhatsApp] {Message}", msg);
			return Task.FromResult(true);
		}

		public Task<bool> SendWeeklySummaryAsync(int userId)
		{
			_logger.LogInformation("[WhatsApp] Enviando resumo semanal para {UserId}", userId);
			return Task.FromResult(true);
		}

		public async Task<bool> SendWhatsAppMessageAsync(string phoneNumber, string message)
		{
			try
			{
				if (string.IsNullOrEmpty(_twilioAccountSid) || string.IsNullOrEmpty(_twilioAuthToken))
				{
					_logger.LogInformation("[WhatsApp] Twilio não configurado - mensagem simulada para {Phone}: {Message}", phoneNumber, message);
					return true;
				}

				var messageResource = await MessageResource.CreateAsync(
					body: message,
					from: new Twilio.Types.PhoneNumber($"whatsapp:{_twilioWhatsAppNumber}"),
					to: new Twilio.Types.PhoneNumber($"whatsapp:{phoneNumber}")
				);

				_logger.LogInformation("[WhatsApp] Mensagem enviada com sucesso: {Sid}", messageResource.Sid);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[WhatsApp] Erro ao enviar mensagem para {Phone}", phoneNumber);
				return false;
			}
		}

		public async Task<bool> ConfigureNotificationSettingsAsync(int userId, UserNotificationSettingsDto settings)
		{
			var entity = await _db.UserNotificationSettings.FirstOrDefaultAsync(s => s.UserId == userId);
			if (entity == null)
			{
				entity = new Domain.Entities.UserNotificationSettings
				{
					UserId = userId,
					WhatsAppEnabled = settings.WhatsAppEnabled,
					NotificationDays = settings.NotificationDays,
					PriceDropThreshold = settings.PriceDropThreshold,
					MaxNotificationsPerDay = settings.MaxNotificationsPerDay,
					IsActive = true
				};
				_db.UserNotificationSettings.Add(entity);
			}
			else
			{
				entity.WhatsAppEnabled = settings.WhatsAppEnabled;
				entity.NotificationDays = settings.NotificationDays;
				entity.PriceDropThreshold = settings.PriceDropThreshold;
				entity.MaxNotificationsPerDay = settings.MaxNotificationsPerDay;
				entity.UpdatedAt = DateTime.UtcNow;
			}
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<NotificationLogDto>> GetUserNotificationHistoryAsync(int userId, int skip = 0, int take = 50)
		{
			var list = await _db.NotificationLogs.AsNoTracking()
				.Where(n => n.UserId == userId)
				.OrderByDescending(n => n.SentAt)
				.Skip(skip).Take(take)
				.Select(n => new NotificationLogDto
				{
					Id = n.Id,
					UserId = n.UserId,
					Type = n.Type,
					Content = n.Content,
					SentAt = n.SentAt,
					Status = n.Status,
					ErrorMessage = n.ErrorMessage,
					Channel = n.Channel ?? "",
					ProductCount = n.ProductCount,
					TotalSavings = n.TotalSavings
				})
				.ToListAsync();
			return list;
		}

		public Task<bool> ProcessDailyNotificationsAsync()
		{
			_logger.LogInformation("[WhatsApp] Processando envio diário de favoritos...");
			return Task.FromResult(true);
		}

		public Task<bool> ProcessPriceDropAlertsAsync()
		{
			_logger.LogInformation("[WhatsApp] Processando alertas de queda de preço...");
			return Task.FromResult(true);
		}
	}
}
