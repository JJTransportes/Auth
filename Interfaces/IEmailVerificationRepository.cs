using Auth.Dtos;
using Auth.Errors;

namespace Auth.Repositories;

public interface IEmailVerificationRepository
{
    Task<Tuple<VerificationDto?, AuthBaseError?>> SendVerificationCodeAsync(
        string email,
        CancellationToken cancellationToken = default
        );

    Task<Tuple<VerificationDto?, AuthBaseError?>> ResendVerificationCodeAsync(
        string email,
        CancellationToken cancellationToken = default
        );
}