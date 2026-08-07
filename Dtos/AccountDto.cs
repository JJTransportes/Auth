using Auth.Enums;

namespace Auth.Dtos;

public record AccountDto(
    Guid Id,
    Guid UserId,
    UserType UserType,
    string Email,
    string VerificationCode
);
