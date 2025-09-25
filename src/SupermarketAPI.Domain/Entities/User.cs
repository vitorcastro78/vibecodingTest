using System;
using System.Collections.Generic;

namespace SupermarketAPI.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public bool IsWhatsAppVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public string? WhatsAppVerificationToken { get; set; }
        
        // Navigation properties
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
        public virtual UserNotificationSettings? NotificationSettings { get; set; }
        public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    }
}
