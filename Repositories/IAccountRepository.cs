using Auth.Dtos;
using Auth.Enums;

namespace Auth.Repositories;

public interface IAccountRepository
{
    Task<AccountDto?> GetByUserAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default);
    Task<AccountDto> CreateAsync(NewAccountDto dto, CancellationToken cancellationToken = default);
    Task<AccountDto> UpdateAsync(Guid userId, UserType userType, UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default);
    Task<string> SendVerificationCodeAsync(string email, CancellationToken cancellationToken = default);
}
