using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<User?> GetByIdAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(UserDto user, string password);
        Task<UserDto> UpdateUserAsync(UserDto user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> VerifyWhatsAppAsync(string token);
        Task<bool> AuthenticateAsync(string email, string password);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
