using Auth.Dtos;
using FluentValidation;

namespace Auth.Validators.Account;

internal sealed class NewAccountValidator : AbstractValidator<NewAccountDto>
{
    public NewAccountValidator()
    {
        RuleFor(x => x.UserType)
            .NotEmpty().WithMessage("O tipo de usuário deve ser informado.");
        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("A senha precisa ter pelo menos 8 dígitos.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("Formato de e-mail inválido.");
        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("O código de verificação é obrigatório.")
            .MaximumLength(6).WithMessage("Código de verificação inválido.");
    }
}