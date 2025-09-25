using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketAPI.Application.Interfaces;
using SupermarketAPI.Domain.Entities;
using SupermarketAPI.Notifications.Services;
using System.Security.Claims;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailNotificationService _emailService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(
            IEmailNotificationService emailService,
            IProductService productService,
            IUserService userService,
            ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _productService = productService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("send-product-results")]
        public async Task<IActionResult> SendProductResults([FromBody] SendProductResultsRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Usuário não encontrado");

                var products = await _productService.GetProductsAsync(request.ProductIds);
                if (!products.Any())
                    return NotFound("Nenhum produto encontrado");

                await _emailService.SendProductResultsEmailAsync(user, products.ToList(), request.Subject);
                
                _logger.LogInformation("Email de resultados enviado para usuário {UserId}", userId);
                return Ok(new { message = "Email enviado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email de resultados");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost("send-price-drop-alert")]
        public async Task<IActionResult> SendPriceDropAlert([FromBody] SendPriceDropAlertRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Usuário não encontrado");

                var product = await _productService.GetByIdAsync(request.ProductId);
                if (product == null)
                    return NotFound("Produto não encontrado");

                await _emailService.SendPriceDropAlertAsync(user, product, request.OldPrice, request.NewPrice);
                
                _logger.LogInformation("Alerta de redução de preço enviado para usuário {UserId}", userId);
                return Ok(new { message = "Alerta enviado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar alerta de redução de preço");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost("send-weekly-summary")]
        public async Task<IActionResult> SendWeeklySummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Usuário não encontrado");

                // Buscar produtos favoritos do usuário
                var favoriteProducts = await _productService.GetUserFavoritesAsync(userId);
                
                // Buscar produtos com redução de preço (simulado)
                var priceDrops = await _productService.GetPriceDropsAsync();

                await _emailService.SendWeeklySummaryAsync(user, favoriteProducts.ToList(), priceDrops.ToList());
                
                _logger.LogInformation("Resumo semanal enviado para usuário {UserId}", userId);
                return Ok(new { message = "Resumo semanal enviado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar resumo semanal");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost("send-daily-ranking")]
        public async Task<IActionResult> SendDailyRanking()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("Usuário não encontrado");

                // Buscar top produtos do dia
                var topProducts = await _productService.GetTopProductsAsync(10);

                await _emailService.SendDailyRankingEmailAsync(user, topProducts.ToList());
                
                _logger.LogInformation("Ranking diário enviado para usuário {UserId}", userId);
                return Ok(new { message = "Ranking diário enviado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar ranking diário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            throw new UnauthorizedAccessException("ID do usuário não encontrado");
        }
    }

    public class SendProductResultsRequest
    {
        public List<int> ProductIds { get; set; } = new();
        public string? Subject { get; set; }
    }

    public class SendPriceDropAlertRequest
    {
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}
