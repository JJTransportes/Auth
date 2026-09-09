using Auth.Data;
using Auth.Dtos;
using Auth.Errors;
using Auth.Errors.EmailVerification;
using Auth.Extensions;
using Auth.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories;

public class EmailVerificationRepository(
    AppDbContext db,
    IValidator<SendVerificationDto> emailValidator) : IEmailVerificationRepository
{
    public async Task<Tuple<VerificationDto?, AuthBaseError?>> SendVerificationCodeAsync(
        string email,
        CancellationToken cancellationToken = default
        )
    {
        var validationResult = await emailValidator.ValidateAsync(new SendVerificationDto(email));
        if (!validationResult.IsValid)
        {
            var validationErros = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

            return Tuple.Create<VerificationDto?, AuthBaseError?>(
                null,
                new EmailVerificationError(
                    "Ops... Houve uma falha durante a veriicação do e-mail.",
                    validationErros,
                    ErrorType.ValidationError
                    )
                );
        }

        var emailVerification = await db.EmailVerifications
            .FirstOrDefaultAsync(emailVerification =>
                emailVerification.Email == email,
                cancellationToken
             );

        if (emailVerification != null)
        {
            if (emailVerification.Used)
            {
                return Tuple.Create<VerificationDto?, AuthBaseError?>(
                    null,
                    new EmailVerificationError(
                        "Ops... O e-mail informado já foi verificado.",
                        [],
                        ErrorType.VerifiedEmail
                        )
                    );
            }

            if (emailVerification.ExpiresAt < DateTime.UtcNow)
            {
                return Tuple.Create<VerificationDto?, AuthBaseError?>(
                    null,
                    new EmailVerificationError(
                        "Ops... O prazo para verificação do e-mail expirou.",
                        [],
                        ErrorType.VerificationExpired
                        )
                    );
            }

            return Tuple.Create<VerificationDto?, AuthBaseError?>(
                 null,
                 new EmailVerificationError(
                     "Ops... Um código de verificação já foi enviado para esse e-mail.",
                     [],
                     ErrorType.VerificationCodeSent
                     )
                 );
        }

        var code = new Random().Next(100000, 999999).ToString();

        var verification = new EmailVerification
        {
            Id = Guid.NewGuid(),
            Email = email,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Used = false
        };

        db.EmailVerifications.Add(verification);
        await db.SaveChangesAsync(cancellationToken);

        var result = Tuple.Create<VerificationDto?, AuthBaseError?>(verification.MapToDto(), null);

        return result;
    }
}