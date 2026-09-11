using Auth.Models;

namespace Auth.Errors.Account;

public class AccountCreationError : AuthBaseError
{
    public AccountCreationError(
        string Message,
        ICollection<string> Errors,
        ErrorType ErrorType) : base(Message, Errors, ErrorType)
    {
    }
}