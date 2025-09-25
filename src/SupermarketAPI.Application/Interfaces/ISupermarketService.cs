using SupermarketAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface ISupermarketService
    {
        Task<IEnumerable<SupermarketDto>> GetSupermarketsAsync();
        Task<SupermarketDto?> GetSupermarketByIdAsync(int id);
        Task<SupermarketDto> CreateSupermarketAsync(SupermarketDto supermarket);
        Task<SupermarketDto> UpdateSupermarketAsync(SupermarketDto supermarket);
        Task<bool> DeleteSupermarketAsync(int id);
        Task<bool> EnableScrapingAsync(int id, bool enabled);
    }
}
