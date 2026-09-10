namespace Auth.Errors;

public enum ErrorType
{
    // EMAIL VERIFICATION ERRORS
    ValidationError,
    VerificationEmailNotFound,
    VerificationCodeSent,
    VerifiedEmail,
    VerificationExpired,
}