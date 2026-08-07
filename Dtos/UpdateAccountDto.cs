namespace Auth.Dtos;

public record UpdateAccountDto(
    string? Email,
    string? Password,
    string? VerificationCode
);
