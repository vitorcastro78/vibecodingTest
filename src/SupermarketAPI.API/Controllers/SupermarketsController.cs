using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupermarketsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;

        public SupermarketsController(SupermarketDbContext db)
        {
            _db = db;
        }

        // GET /api/supermarkets
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _db.Supermarkets.AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => new SupermarketDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    BaseUrl = s.BaseUrl,
                    LogoUrl = s.LogoUrl,
                    LastScrapedAt = s.LastScrapedAt,
                    IsScrapingEnabled = s.IsScrapingEnabled,
                    ScrapingIntervalMinutes = s.ScrapingIntervalMinutes,
                    ProductCount = s.ProductPrices.Select(pp => pp.ProductId).Distinct().Count(),
                    AvailableProductsCount = s.ProductPrices.Where(pp => pp.IsAvailable).Select(pp => pp.ProductId).Distinct().Count()
                })
                .ToListAsync();
            return Ok(list);
        }
    }
}


