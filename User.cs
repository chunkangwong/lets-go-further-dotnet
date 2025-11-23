// type User struct {
// 	ID        int64     `json:"id"`
// 	CreatedAt time.Time `json:"created_at"`
// 	Name      string    `json:"name"`
// 	Email     string    `json:"email"`
// 	Password  password  `json:"-"`
// 	Activated bool      `json:"activated"`
// 	Version   int       `json:"version"`
// }

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace controller_api_test;

public class User
{
    [Column("id")]
    public int Id { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("activated")]
    public bool Activated { get; set; }

    [Column("version")]
    public int Version { get; set; }
}

public class CreateUserDto
{
    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class ActivateUserDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class LoginUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}