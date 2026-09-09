namespace Auth.Errors.EmailVerification;

public class EmailVerificationError : AuthBaseError
{
    public EmailVerificationError(
        string Message,
        ICollection<string> Errors,
        ErrorType ErrorType) : base(Message, Errors, ErrorType)
    {
    }
}