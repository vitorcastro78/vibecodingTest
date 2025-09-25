using System;

namespace SupermarketAPI.Domain.Entities
{
    public class UserNotificationSettings : BaseEntity
    {
        public int UserId { get; set; }
        public bool WhatsAppEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = false;
        public TimeSpan? NotificationTime { get; set; } // Horário preferido para notificações
        public string NotificationDays { get; set; } = "1,2,3,4,5"; // Dias da semana (1=Segunda, 7=Domingo)
        public decimal PriceDropThreshold { get; set; } = 10.0m; // Percentual mínimo para alerta de queda
        public int MaxNotificationsPerDay { get; set; } = 5;
        public bool DailyFavoritesEnabled { get; set; } = true;
        public bool PriceDropAlertsEnabled { get; set; } = true;
        public bool WeeklySummaryEnabled { get; set; } = true;
        public DateTime? LastNotificationSent { get; set; }
        public int NotificationsSentToday { get; set; } = 0;
        public DateTime? LastResetDate { get; set; } // Para resetar contador diário
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
