using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AuthService.Base;
using AuthService.Entities;
using AuthService.IServices;
using AuthService.Models;
using Core.Entities;
using Infrastructure.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Core;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JWT _jwt;
    private readonly Logger _logger;
    public AuthService(UserManager<AppUser> userManager, IOptions<JWT> jwt,
        RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _jwt = jwt.Value;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }

    public async Task<AuthModel> RegisterUserAsync(RegisterModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        if (_userManager == null) throw new InvalidOperationException("UserManager is not initialized.");

        if (string.IsNullOrWhiteSpace(model.Username) || !Regex.IsMatch(model.Username, @"^[a-zA-Z0-9]+$"))
        {
            return new AuthModel
            {
                Message = "Username can only contain letters and digits and cannot be empty.",
                IsAuthenticated = false
            };
        }

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
            return new AuthModel { Message = "Email is already registered", IsAuthenticated = false };

        if (await _userManager.FindByNameAsync(model.Username) is not null)
            return new AuthModel { Message = "UserName is already registered", IsAuthenticated = false };

        var user = new AppUser
        {
            Email = model.Email,
            UserName = model.Username,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var UserCreation = await _userManager.CreateAsync(user, model.Password);

        var RoleSetting = await _userManager.AddToRoleAsync(user, "User");

        if (!UserCreation.Succeeded || !RoleSetting.Succeeded)
        {

            var errors = string.Empty;
            foreach (var error in ((!UserCreation.Succeeded) ? UserCreation : RoleSetting).Errors)
            {
                errors += $"{error.Description},";
            }
            return new AuthModel { Message = errors };
        }
        var jwtSecurityToken = await CreateJwtToken(user);
        user.RefreshTokenExpiryTime = jwtSecurityToken.ValidTo;
        user.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        user.RefreshTokens.Add(GenerateRefreshToken());

        _logger.Information("User {UserId} registered successfully with roles: {Roles}", user.Id, "User");
        return new AuthModel
        {
            Message = $"User {user.Email} registered Successfully as User",
            User = user,
            Email = user.Email,
            IsAuthenticated = true,
            Roles = new List<string> { "User" },
            Username = user.UserName,
            Token = user.Token,
            RefreshToken = user.RefreshTokens.SingleOrDefault(t => t.IsActive)?.Token,
            RefreshTokenExpiryTime = user.RefreshTokenExpiryTime
        };
    }

    public async Task<AuthModel> LoginAsync(TokenRequestModel model)
    {
        var authModel = new AuthModel();

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            Thread.Sleep(1000); // Adding delay to prevent time difference attacks
            authModel.Message = "Email or Password is incorrect!";
            return authModel;
        }

        var jwtsecurityToken = await CreateJwtToken(user);
        var RefreshTokenExpiryTime = user.RefreshTokenExpiryTime;

        var rolesList = await _userManager.GetRolesAsync(user);

        if (authModel.User == null)
        {
            authModel.User = new AppUser();
        }
        authModel.IsAuthenticated = true;
        authModel.User.Token = new JwtSecurityTokenHandler().WriteToken(jwtsecurityToken);
        authModel.Email = user.Email;
        authModel.Username = user.UserName;
        authModel.User.RefreshTokenExpiryTime = RefreshTokenExpiryTime;
        authModel.Roles = rolesList.ToList();

        if (user.RefreshTokens.Any(t => t.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            authModel.RefreshToken = activeRefreshToken.Token;
            authModel.RefreshTokenExpiryTime = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiryTime = newRefreshToken.ExpiresOn;
            await _userManager.UpdateAsync(user);
        }
        _logger.Information("User {UserId} logged in successfully with roles: {Roles}", user.Id, string.Join(", ", rolesList));
        return authModel;
    }

    public async Task<AddRoleResult> AddRoleAsync(AddRoleModel model)
    {
        var res = new AddRoleResult();
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());

        if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
        {
            res.Message = "Invalid user ID or Role";
            return res;
        }
        if (await _userManager.IsInRoleAsync(user, model.Role))
        {
            _logger.Information("User {UserId} is already assigned to role {Role}", model.UserId, model.Role);
            res.Message = "User already assigned to this role";
            return res;
        }

        var AddRoleresult = await _userManager.AddToRoleAsync(user, model.Role);
        if (AddRoleresult == null || !AddRoleresult.Succeeded)
        {
            _logger.Error("Failed to add role {Role} to user {UserId}: {Errors}", model.Role, model.UserId, AddRoleresult?.Errors);
            res.Message = "Failed to add role to user";
            return res;
        }
        var jwtSecurityToken = await CreateJwtToken(user);
        user.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        user.RefreshTokenExpiryTime = jwtSecurityToken.ValidTo;

        var updateResult = await _userManager.UpdateAsync(user);
        if (updateResult != null && updateResult.Succeeded)
        {
            _logger.Information("User {UserId} successfully added to role {Role}", model.UserId, model.Role);
            _logger.Information("New JWT token created for user {UserId} with expiration at {Expiration}", model.UserId, jwtSecurityToken.ValidTo);
        }
        res.Message = "Role added successfully";
        res.IsSuccess = true;
        _logger.Information("Role {Role} added to user {UserId}", model.Role, model.UserId);
        return res;
    }

    private async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        // _logger.Information("Key: {0}", Encoding.UTF8.GetBytes(_jwt.Key)); 
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signingCredentials);

        return jwtSecurityToken;
    }
    private async Task<bool> AuthenticatedUser(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwt.Key);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    // used when the token is expired and want to refresh it
    private async Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
    {
        var SecKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        if (Encoding.UTF8.GetByteCount(_jwt.Key) >= 16)
        {
            SecKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        }
        else
        {
            var keyBytes = Encoding.UTF8.GetBytes(_jwt.Key);
            Array.Resize(ref keyBytes, 16);
            SecKey = new SymmetricSecurityKey(keyBytes);
        }
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SecKey,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
    public async Task<UserRoleDto> GetRoleAsync(GetRoleModel model)
    {

        var user = await _userManager.FindByIdAsync(model.UserId);
        var roles = await _userManager.GetRolesAsync(user);

        return new UserRoleDto()
        {
            Roles = roles
        };
    }

    public async Task<AuthModel> RefreshTokenAsync(string token)
    {
        var authModel = new AuthModel();

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
        {
            authModel.Message = "Invalid token";
            return authModel;
        }

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
        {
            authModel.Message = "Inactive token";
            return authModel;
        }

        refreshToken.RevokedOn = DateTime.UtcNow;

        var newRefreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        var jwtToken = await CreateJwtToken(user);
        authModel.IsAuthenticated = true;
        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        authModel.Email = user.Email;
        authModel.Username = user.UserName;
        var roles = await _userManager.GetRolesAsync(user);
        authModel.Roles = roles.ToList();
        authModel.RefreshToken = newRefreshToken.Token;
        authModel.RefreshTokenExpiryTime = newRefreshToken.ExpiresOn;

        return authModel;
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
            return false;

        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
            return false;

        refreshToken.RevokedOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return true;
    }

    private RefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}