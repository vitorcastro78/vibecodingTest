using SupermarketAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface IRankingService
    {
        Task<IEnumerable<RankingDto>> GetDailyRankingsAsync(DateTime? date = null);
        Task<RankingDto?> GetRankingByCategoryAsync(int categoryId, DateTime? date = null);
        Task<IEnumerable<RankingProductDto>> GetTopProductsByCategoryAsync(int categoryId, int take = 10);
        Task<IEnumerable<RankingProductDto>> GetBestDealsAsync(int take = 20);
        Task<IEnumerable<RankingProductDto>> GetPriceDropsAsync(decimal thresholdPercentage = 10, int take = 20);
        Task<RankingDto> GenerateDailyRankingAsync(int categoryId);
        Task<bool> GenerateAllRankingsAsync();
    }
}
