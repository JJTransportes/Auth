using Auth.Dtos;
using Auth.Models;

namespace Auth.Extensions;

public static class EmailVerificationExtensions
{
    public static VerificationDto MapToDto(this EmailVerification emailVerification)
    {
        return new VerificationDto(
            emailVerification.Id,
            emailVerification.Email,
            emailVerification.CreatedAt,
            emailVerification.ExpiresAt
        );
    }
}