using Auth.Dtos;
using Auth.Models;

namespace Auth.Extensions;

public static class AccountExtensions
{
    public static AccountDto MapToDto(this Account account)
    {
        return new AccountDto(
            account.Id,
            account.UserId,
            account.UserType,
            account.Email,
            account.VerificationCode
        );
    }
}
