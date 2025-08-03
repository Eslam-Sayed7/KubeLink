using AuthService.Base;
using AuthService.Entities;
using AuthService.IServices;
using AuthService.Models;
using Infrastructure.Base;

namespace AuthService.Services;

public class UserService : IUserService
{
    private  readonly IUnitOfWork _unitOfWork;
    
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

   public async Task<AppUser> GetUserByIdAsync(Guid id)
  {
      var user = await _unitOfWork.Repository<AppUser>().GetByIdAsync(id);
      return user;
  }

  public Task<AppUser> UpdateUserAsync(UpdateUserModel model)
  {
      throw new NotImplementedException();
  }
}