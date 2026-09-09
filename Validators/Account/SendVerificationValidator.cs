using Auth.Dtos;
using Auth.Repositories;
using FluentValidation;

namespace Auth.Validators.Account;

internal sealed class SendVerificationValidator : AbstractValidator<SendVerificationDto>
{
    public SendVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("O e-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("Formato de e-mail inválido.");
    }
}