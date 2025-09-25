using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Domain.Entities;
using System.Linq;

namespace SupermarketAPI.Application.Mappers
{
    public static class ProductMapper
    {
        public static ProductDto ToDto(Product product)
        {
            if (product == null) return null!;

            return new ProductDto
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
                Prices = product.ProductPrices?.Select(pp => ProductPriceMapper.ToDto(pp)).ToList() ?? new List<ProductPriceDto>()
            };
        }

        public static Product ToEntity(ProductDto dto)
        {
            if (dto == null) return null!;

            return new Product
            {
                Id = dto.Id,
                Name = dto.Name,
                Brand = dto.Brand,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Barcode = dto.Barcode,
                CategoryId = dto.CategoryId,
                AveragePrice = dto.AveragePrice,
                MinPrice = dto.MinPrice,
                MaxPrice = dto.MaxPrice,
                LastPriceUpdate = dto.LastPriceUpdate
            };
        }
    }

    public static class ProductPriceMapper
    {
        public static ProductPriceDto ToDto(ProductPrice price)
        {
            if (price == null) return null!;

            return new ProductPriceDto
            {
                Id = price.Id,
                ProductId = price.ProductId,
                SupermarketId = price.SupermarketId,
                SupermarketName = price.Supermarket?.Name ?? string.Empty,
                SupermarketLogoUrl = price.Supermarket?.LogoUrl ?? string.Empty,
                Price = price.Price,
                Unit = price.Unit,
                OriginalUnit = price.OriginalUnit,
                PricePerUnit = price.PricePerUnit,
                Url = price.Url,
                ScrapedAt = price.ScrapedAt,
                IsAvailable = price.IsAvailable,
                IsOnSale = price.IsOnSale,
                OriginalPrice = price.OriginalPrice,
                SaleEndDate = price.SaleEndDate,
                SaleDescription = price.SaleDescription,
                DiscountPercentage = price.IsOnSale && price.OriginalPrice.HasValue 
                    ? ((price.OriginalPrice.Value - price.Price) / price.OriginalPrice.Value) * 100 
                    : null
            };
        }

        public static ProductPrice ToEntity(ProductPriceDto dto)
        {
            if (dto == null) return null!;

            return new ProductPrice
            {
                Id = dto.Id,
                ProductId = dto.ProductId,
                SupermarketId = dto.SupermarketId,
                Price = dto.Price,
                Unit = dto.Unit,
                OriginalUnit = dto.OriginalUnit,
                PricePerUnit = dto.PricePerUnit,
                Url = dto.Url,
                ScrapedAt = dto.ScrapedAt,
                IsAvailable = dto.IsAvailable,
                IsOnSale = dto.IsOnSale,
                OriginalPrice = dto.OriginalPrice,
                SaleEndDate = dto.SaleEndDate,
                SaleDescription = dto.SaleDescription
            };
        }
    }
}
