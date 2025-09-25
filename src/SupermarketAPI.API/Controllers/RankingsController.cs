using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;

        public RankingsController(SupermarketDbContext db)
        {
            _db = db;
        }

        // GET /api/rankings/daily?date=2025-01-01
        [HttpGet("daily")]
        public async Task<IActionResult> Daily([FromQuery] DateTime? date = null)
        {
            var d = (date ?? DateTime.UtcNow).Date;
            var rankings = await _db.DailyRankings.AsNoTracking()
                .Where(r => r.Date.Date == d)
                .Include(r => r.Category)
                .OrderBy(r => r.Category.Name)
                .Select(r => new RankingDto
                {
                    Id = r.Id,
                    Date = r.Date,
                    CategoryId = r.CategoryId,
                    CategoryName = r.Category.Name,
                    TotalProducts = r.TotalProducts,
                    AveragePrice = r.AveragePrice,
                    MinPrice = r.MinPrice,
                    MaxPrice = r.MaxPrice,
                    SupermarketsCount = r.SupermarketsCount
                })
                .ToListAsync();
            return Ok(rankings);
        }

        // GET /api/rankings/category/{category}
        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> ByCategory([FromRoute] int categoryId, [FromQuery] DateTime? date = null)
        {
            var d = (date ?? DateTime.UtcNow).Date;
            var r = await _db.DailyRankings.AsNoTracking()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.CategoryId == categoryId && x.Date.Date == d);
            if (r == null) return NotFound();

            var dto = new RankingDto
            {
                Id = r.Id,
                Date = r.Date,
                CategoryId = r.CategoryId,
                CategoryName = r.Category?.Name ?? string.Empty,
                TotalProducts = r.TotalProducts,
                AveragePrice = r.AveragePrice,
                MinPrice = r.MinPrice,
                MaxPrice = r.MaxPrice,
                SupermarketsCount = r.SupermarketsCount
            };
            return Ok(dto);
        }
    }
}


