using AuthService.Models;
using Infrastructure.Dtos;

namespace AuthService.IServices;
    public interface IAuthService {
        Task<AuthModel> RegisterUserAsync(RegisterModel model);

        Task<AuthModel> LoginAsync(TokenRequestModel model);

        Task<AddRoleResult> AddRoleAsync(AddRoleModel model);

        Task<UserRoleDto> GetRoleAsync(GetRoleModel model);
        Task<AuthModel> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);

    }
