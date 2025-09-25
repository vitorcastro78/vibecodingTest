using SupermarketAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<IEnumerable<UserFavoriteDto>> GetUserFavoritesAsync(int userId);
        Task<UserFavoriteDto> AddFavoriteAsync(int userId, int productId, decimal? priceThreshold = null);
        Task<bool> RemoveFavoriteAsync(int userId, int productId);
        Task<bool> UpdateFavoriteThresholdAsync(int userId, int productId, decimal? priceThreshold);
        Task<bool> TogglePriceAlertAsync(int userId, int productId, bool enabled);
        Task<IEnumerable<UserFavoriteDto>> GetFavoritesWithPriceAlertsAsync(int userId);
    }
}
