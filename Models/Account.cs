using Auth.Enums;

namespace Auth.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserType UserType { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
}
