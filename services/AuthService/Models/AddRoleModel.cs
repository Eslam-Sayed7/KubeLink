using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;
public class AddRoleModel
{
        [Required]
        public Guid  UserId { get; set; }

        [Required]
        public string Role { get; set; }

}