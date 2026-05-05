using System.ComponentModel.DataAnnotations;

namespace ConnectHub.Auth.Domain;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public bool IsActive { get; set; } = true;
}