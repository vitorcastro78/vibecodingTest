using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Domain.Enums;
using System.Net;
using System.Net.Mail;

namespace SupermarketAPI.Notifications.Services
{
    public interface IEmailNotificationService
    {
        Task SendProductResultsEmailAsync(User user, List<Product> products, string subject = null);
        Task SendPriceDropAlertAsync(User user, Product product, decimal oldPrice, decimal newPrice);
        Task SendWeeklySummaryAsync(User user, List<Product> favoriteProducts, List<Product> priceDrops);
        Task SendDailyRankingEmailAsync(User user, List<Product> topProducts);
    }

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly EmailSettings _emailSettings;
        private readonly IProductService _productService;

        public EmailNotificationService(
            ILogger<EmailNotificationService> logger,
            IOptions<EmailSettings> emailSettings,
            IProductService productService)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
            _productService = productService;
        }

        public async Task SendProductResultsEmailAsync(User user, List<Product> products, string? subject = null)
        {
            try
            {
                var emailSubject = subject ?? "Resultados de Produtos - SupermarketAPI";
                var emailBody = GenerateProductResultsEmailBody(user, products);

                await SendEmailAsync(user.Email, emailSubject, emailBody);
                
                _logger.LogInformation("Email de resultados enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de resultados para {Email}", user.Email);
                throw;
            }
        }

        public async Task SendPriceDropAlertAsync(User user, Product product, decimal oldPrice, decimal newPrice)
        {
            try
            {
                var subject = "🚨 Alerta de Redução de Preço - SupermarketAPI";
                var emailBody = GeneratePriceDropEmailBody(user, product, oldPrice, newPrice);

                await SendEmailAsync(user.Email, subject, emailBody);
                
                _logger.LogInformation("Alerta de redução de preço enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar alerta de redução de preço para {Email}", user.Email);
                throw;
            }
        }

        public async Task SendWeeklySummaryAsync(User user, List<Product> favoriteProducts, List<Product> priceDrops)
        {
            try
            {
                var subject = "📊 Resumo Semanal - SupermarketAPI";
                var emailBody = GenerateWeeklySummaryEmailBody(user, favoriteProducts, priceDrops);

                await SendEmailAsync(user.Email, subject, emailBody);
                
                _logger.LogInformation("Resumo semanal enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar resumo semanal para {Email}", user.Email);
                throw;
            }
        }

        public async Task SendDailyRankingEmailAsync(User user, List<Product> topProducts)
        {
            try
            {
                var subject = "🏆 Ranking Diário - SupermarketAPI";
                var emailBody = GenerateDailyRankingEmailBody(user, topProducts);

                await SendEmailAsync(user.Email, subject, emailBody);
                
                _logger.LogInformation("Ranking diário enviado para {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar ranking diário para {Email}", user.Email);
                throw;
            }
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);
            await client.SendMailAsync(message);
        }

        private string GenerateProductResultsEmailBody(User user, List<Product> products)
        {
            var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : "Usuário";
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Resultados de Produtos</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .header h1 {{ color: #2c3e50; margin: 0; }}
        .product-card {{ border: 1px solid #ddd; margin: 15px 0; padding: 15px; border-radius: 8px; background: #fafafa; }}
        .product-name {{ font-size: 18px; font-weight: bold; color: #2c3e50; margin-bottom: 10px; }}
        .product-price {{ font-size: 24px; color: #e74c3c; font-weight: bold; }}
        .product-details {{ margin-top: 10px; color: #666; }}
        .supermarket {{ background: #3498db; color: white; padding: 4px 8px; border-radius: 4px; font-size: 12px; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🛒 Resultados de Produtos</h1>
            <p>Olá {userName}, aqui estão os produtos que você pesquisou:</p>
        </div>
        
        <div class='products'>
";

            foreach (var product in products.Take(10)) // Limitar a 10 produtos
            {
                html += $@"
            <div class='product-card'>
                <div class='product-name'>{product.Name}</div>
                <div class='product-price'>€{product.AveragePrice:F2}</div>
                <div class='product-details'>
                    <span class='supermarket'>{product.Category?.Name ?? "Geral"}</span>
                    <span style='margin-left: 10px;'>Categoria: {product.Category?.Name ?? "Geral"}</span>
                </div>
            </div>";
            }

            html += @"
        </div>
        
        <div class='footer'>
            <p>Este email foi enviado automaticamente pelo SupermarketAPI</p>
            <p>Para parar de receber estes emails, acesse suas configurações de notificação</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }

        private string GeneratePriceDropEmailBody(User user, Product product, decimal oldPrice, decimal newPrice)
        {
            var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : "Usuário";
            var savings = oldPrice - newPrice;
            var savingsPercentage = (savings / oldPrice) * 100;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Alerta de Redução de Preço</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .alert {{ background: #e74c3c; color: white; padding: 15px; border-radius: 8px; margin-bottom: 20px; text-align: center; }}
        .product-card {{ border: 2px solid #e74c3c; margin: 15px 0; padding: 20px; border-radius: 8px; background: #fff5f5; }}
        .product-name {{ font-size: 20px; font-weight: bold; color: #2c3e50; margin-bottom: 15px; }}
        .price-comparison {{ display: flex; justify-content: space-between; margin: 15px 0; }}
        .old-price {{ text-decoration: line-through; color: #666; font-size: 18px; }}
        .new-price {{ color: #e74c3c; font-size: 24px; font-weight: bold; }}
        .savings {{ background: #27ae60; color: white; padding: 10px; border-radius: 4px; text-align: center; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='alert'>
            <h1>🚨 ALERTA DE REDUÇÃO DE PREÇO!</h1>
        </div>
        
        <div class='product-card'>
            <div class='product-name'>{product.Name}</div>
            <div class='price-comparison'>
                <div class='old-price'>€{oldPrice:F2}</div>
                <div class='new-price'>€{newPrice:F2}</div>
            </div>
            <div class='savings'>
                <strong>Poupe €{savings:F2} ({savingsPercentage:F1}% de desconto!)</strong>
            </div>
        </div>
        
        <div style='text-align: center; margin-top: 30px;'>
            <p>Olá {userName}, o preço do produto que você está acompanhando reduziu!</p>
            <p>Aproveite esta oportunidade!</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWeeklySummaryEmailBody(User user, List<Product> favoriteProducts, List<Product> priceDrops)
        {
            var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : "Usuário";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Resumo Semanal</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .section {{ margin: 30px 0; }}
        .section h2 {{ color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }}
        .product-card {{ border: 1px solid #ddd; margin: 15px 0; padding: 15px; border-radius: 8px; background: #fafafa; }}
        .stats {{ display: flex; justify-content: space-around; margin: 20px 0; }}
        .stat {{ text-align: center; padding: 20px; background: #ecf0f1; border-radius: 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📊 Resumo Semanal - SupermarketAPI</h1>
            <p>Olá {userName}, aqui está o resumo da sua semana:</p>
        </div>
        
        <div class='stats'>
            <div class='stat'>
                <h3>{favoriteProducts.Count}</h3>
                <p>Produtos Favoritos</p>
            </div>
            <div class='stat'>
                <h3>{priceDrops.Count}</h3>
                <p>Reduções de Preço</p>
            </div>
        </div>
        
        <div class='section'>
            <h2>🛒 Seus Produtos Favoritos</h2>
            {GenerateProductListHtml(favoriteProducts.Take(5))}
        </div>
        
        <div class='section'>
            <h2>💰 Reduções de Preço</h2>
            {GenerateProductListHtml(priceDrops.Take(5))}
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateDailyRankingEmailBody(User user, List<Product> topProducts)
        {
            var userName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : "Usuário";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Ranking Diário</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 800px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .ranking-item {{ display: flex; align-items: center; margin: 15px 0; padding: 15px; background: #f8f9fa; border-radius: 8px; }}
        .rank {{ font-size: 24px; font-weight: bold; color: #3498db; margin-right: 20px; min-width: 40px; }}
        .product-info {{ flex: 1; }}
        .product-name {{ font-size: 18px; font-weight: bold; color: #2c3e50; }}
        .product-price {{ font-size: 20px; color: #e74c3c; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🏆 Ranking Diário - SupermarketAPI</h1>
            <p>Olá {userName}, aqui estão os produtos mais baratos de hoje:</p>
        </div>
        
        <div class='ranking'>
            {GenerateRankingListHtml(topProducts.Take(10))}
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateProductListHtml(IEnumerable<Product> products)
        {
            var html = "";
            foreach (var product in products)
            {
                html += $@"
            <div class='product-card'>
                <div class='product-name'>{product.Name}</div>
                <div class='product-price'>€{product.AveragePrice:F2}</div>
            </div>";
            }
            return html;
        }

        private string GenerateRankingListHtml(IEnumerable<Product> products)
        {
            var html = "";
            var rank = 1;
            foreach (var product in products)
            {
                html += $@"
            <div class='ranking-item'>
                <div class='rank'>#{rank}</div>
                <div class='product-info'>
                    <div class='product-name'>{product.Name}</div>
                    <div class='product-price'>€{product.AveragePrice:F2}</div>
                </div>
            </div>";
                rank++;
            }
            return html;
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "noreply@supermarketapi.com";
        public string FromName { get; set; } = "SupermarketAPI";
        public bool EnableSsl { get; set; } = true;
    }
}
