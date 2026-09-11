using Auth.Dtos;
using Auth.Enums;
using Auth.Errors;

namespace Auth.Interfaces;

public interface IAccountRepository
{
    Task<Tuple<AccountDto?, AuthBaseError?>> CreateAsync(NewAccountDto dto, CancellationToken cancellationToken = default);
    Task<Tuple<AccountDto?, AuthBaseError?>> GetByUserAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default);
    Task<Tuple<AccountDto?, AuthBaseError?>> UpdateAsync(Guid userId, UserType userType, UpdateAccountDto dto, CancellationToken cancellationToken = default);
    Task<Tuple<bool, AuthBaseError?>> DeleteAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default);
}
