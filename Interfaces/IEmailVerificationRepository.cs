using Auth.Dtos;
using Auth.Errors;

namespace Auth.Repositories;

public interface IEmailVerificationRepository
{
    Task<Tuple<VerificationDto?, AuthBaseError?>> SendVerificationCodeAsync(
        string email,
        CancellationToken cancellationToken = default
        );
}