namespace Infrastructure.Dtos;

public class UserRoleDto
{
    public IList<string>? Roles { get; set; }

    public UserRoleDto()
    {
        Roles = new List<string>();
    }
}