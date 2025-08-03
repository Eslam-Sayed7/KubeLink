using AuthService.Entities;
using AuthService.Models;

namespace AuthService.IServices;

public interface IUserService
{
    Task<AppUser> GetUserByIdAsync(Guid id);
    // Task<bool> ValidTokenAsync(string token);
    Task<AppUser> UpdateUserAsync(UpdateUserModel model);
}