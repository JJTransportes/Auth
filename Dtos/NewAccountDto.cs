using Auth.Enums;

namespace Auth.Dtos;

public record NewAccountDto(
    UserType UserType,
    string Email,
    string Password,
    string VerificationCode
);
