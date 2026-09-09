namespace Auth.Dtos;

public record VerificationDto(
Guid Id,
string Email,
DateTime CreatedAt,
DateTime ExpiresAt
);