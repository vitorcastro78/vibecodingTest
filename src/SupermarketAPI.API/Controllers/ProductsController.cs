using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Infrastructure.Data;

namespace SupermarketAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly SupermarketDbContext _db;

        public ProductsController(SupermarketDbContext db)
        {
            _db = db;
        }

        // GET /api/products
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int skip = 0, [FromQuery] int take = 50, [FromQuery] int? categoryId = null)
        {
            var query = _db.Products.AsNoTracking().Where(p => p.IsActive);
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);

            var products = await query
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Barcode = p.Barcode,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    AveragePrice = p.AveragePrice,
                    MinPrice = p.MinPrice,
                    MaxPrice = p.MaxPrice,
                    LastPriceUpdate = p.LastPriceUpdate
                })
                .ToListAsync();
            return Ok(products);
        }

        // GET /api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                AveragePrice = product.AveragePrice,
                MinPrice = product.MinPrice,
                MaxPrice = product.MaxPrice,
                LastPriceUpdate = product.LastPriceUpdate,
                Prices = product.ProductPrices.Select(pp => new ProductPriceDto
                {
                    Id = pp.Id,
                    ProductId = pp.ProductId,
                    SupermarketId = pp.SupermarketId,
                    SupermarketName = pp.Supermarket.Name,
                    SupermarketLogoUrl = pp.Supermarket.LogoUrl,
                    Price = pp.Price,
                    Unit = pp.Unit,
                    OriginalUnit = pp.OriginalUnit,
                    PricePerUnit = pp.PricePerUnit,
                    Url = pp.Url,
                    ScrapedAt = pp.ScrapedAt,
                    IsAvailable = pp.IsAvailable,
                    IsOnSale = pp.IsOnSale,
                    OriginalPrice = pp.OriginalPrice,
                    SaleEndDate = pp.SaleEndDate,
                    SaleDescription = pp.SaleDescription
                }).OrderBy(pp => pp.Price).ToList()
            };
            return Ok(dto);
        }

        // GET /api/products/search
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<ProductDto>());
            var term = q.ToLower();
            var products = await _db.Products.AsNoTracking()
                .Where(p => p.IsActive && (
                    p.Name.ToLower().Contains(term) ||
                    p.NormalizedName.ToLower().Contains(term) ||
                    p.Brand.ToLower().Contains(term)))
                .OrderBy(p => p.Name)
                .Skip(skip)
                .Take(take)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Barcode = p.Barcode,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    AveragePrice = p.AveragePrice,
                    MinPrice = p.MinPrice,
                    MaxPrice = p.MaxPrice,
                    LastPriceUpdate = p.LastPriceUpdate
                })
                .ToListAsync();
            return Ok(products);
        }

        // GET /api/products/compare?productId=123
        [HttpGet("compare")]
        public async Task<IActionResult> Compare([FromQuery] int productId)
        {
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.ProductPrices.Where(pp => pp.IsAvailable))
                .ThenInclude(pp => pp.Supermarket)
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
            if (product == null) return NotFound();

            var prices = product.ProductPrices
                .OrderBy(pp => pp.Price)
                .Select(pp => new ProductPriceDto
                {
                    Id = pp.Id,
                    ProductId = pp.ProductId,
                    SupermarketId = pp.SupermarketId,
                    SupermarketName = pp.Supermarket.Name,
                    SupermarketLogoUrl = pp.Supermarket.LogoUrl,
                    Price = pp.Price,
                    Unit = pp.Unit,
                    OriginalUnit = pp.OriginalUnit,
                    PricePerUnit = pp.PricePerUnit,
                    Url = pp.Url,
                    ScrapedAt = pp.ScrapedAt,
                    IsAvailable = pp.IsAvailable,
                    IsOnSale = pp.IsOnSale,
                    OriginalPrice = pp.OriginalPrice,
                    SaleEndDate = pp.SaleEndDate,
                    SaleDescription = pp.SaleDescription
                })
                .ToList();

            return Ok(new { productId = product.Id, product.Name, prices });
        }
    }
}


