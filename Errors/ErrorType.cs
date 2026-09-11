namespace Auth.Errors;

public enum ErrorType
{
    // EMAIL VERIFICATION ERRORS
    ValidationError,
    VerificationEmailNotFound,
    VerificationCodeSent,
    VerifiedEmail,
    VerificationExpired,

    // ACCOUNT CREATION ERRORS
    InvalidAccountData,
    AccountEmailNotFound,
    InvalidVerificationCode,
    EmailAlreadyRegistered,
    AccountNotFound
}