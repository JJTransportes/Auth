using Auth.Dtos;
using Auth.Enums;
using Auth.Errors;
using Auth.Interfaces;
using Auth.Repositories;

namespace Auth.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounts");

        group.MapPost("/verify-email", async (
            SendVerificationDto dto,
            IEmailVerificationRepository repository,
            CancellationToken ct) =>
        {
            var (emailVerification, error) = await repository.SendVerificationCodeAsync(dto.Email, ct);

            return emailVerification != null ?
                _BuildVerificationCodeSentResponse(emailVerification) :
                _BuildVerificationCodeSendingFailureResponse(error!);
        });

        group.MapPost("/resend-verification", async (
            SendVerificationDto dto,
            IEmailVerificationRepository repository,
            CancellationToken ct) =>
        {
            var (emailVerification, error) = await repository.ResendVerificationCodeAsync(dto.Email, ct);

            return emailVerification != null ?
                _BuildVerificationCodeResentResponse(emailVerification) :
                _BuildVerificationCodeResendingFailureResponse(error!);
        });

        group.MapPost("/", async (
            NewAccountDto dto,
            IAccountRepository repository,
            CancellationToken cancellationToken) =>
        {

            var (newAccount, error) = await repository.CreateAsync(dto, cancellationToken);
            return newAccount != null ?
                _BuildAccountCreationResponse(newAccount) :
                _BuildAccountCreationFailureResponse(error!);
        });

        group.MapGet("/{userType}/{userId:guid}", async (
            UserType userType,
            Guid userId,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            var account = await repository.GetByUserAsync(userId, userType, ct);
            return account is null ? Results.NotFound() : Results.Ok(account);
        });

        group.MapPut("/{userType}/{userId:guid}", async (
            Guid userId,
            UserType userType,
            UpdateAccountDto dto,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            var (updated, error) = await repository.UpdateAsync(userId, userType, dto, ct);
            return updated is not null ?
            Results.Ok(new ResponseDto<AccountDto>
            {
                Data = updated,
                Message = "Conta atualizada com sucesso!"
            }) :
            Results.NotFound(new ResponseDto<AccountDto?>
            {
                Data = null,
                Message = "Ops... Houve um erro na atualização da conta. Verifique os dados e tente novamente."
            });
        });

        group.MapDelete("/{userType}/{userId:guid}", async (
            Guid userId,
            UserType userType,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            var (deleted, _) = await repository.DeleteAsync(userId, userType, ct);
            return deleted ? Results.NoContent() :
            Results.NotFound(
            new ResponseDto<AccountDto?>
            {
                Data = null,
                Message = "Ops... Houve um erro ao deletar sua conta. Verifique os dados e tente novamente."
            });
        });
    }

    private static IResult _BuildVerificationCodeSentResponse(VerificationDto emailVerification)
    {
        return Results.Ok(new ResponseDto<VerificationDto?>()
        {
            Data = emailVerification,
            Message = $"Código de verificação enviado para {emailVerification.Email}.",
        });
    }

    private static IResult _BuildVerificationCodeSendingFailureResponse(AuthBaseError error)
    {
        var result = error.ErrorType switch
        {
            ErrorType.ValidationError => Results.BadRequest(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerifiedEmail => Results.Conflict(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationExpired => Results.UnprocessableEntity(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationCodeSent => Results.Conflict(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            _ => Results.InternalServerError(new ResponseDto<string?>()
            {
                Data = null,
                Message = "Erro desconhecido ao verificar e-mail.",
            })
        };

        return result;
    }

    private static IResult _BuildVerificationCodeResentResponse(VerificationDto emailVerification)
    {
        return Results.Ok(new ResponseDto<VerificationDto?>()
        {
            Data = emailVerification,
            Message = $"Código de verificação enviado para {emailVerification.Email}.",
        });
    }

    private static IResult _BuildVerificationCodeResendingFailureResponse(AuthBaseError error)
    {
        var result = error.ErrorType switch
        {
            ErrorType.ValidationError => Results.BadRequest(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationEmailNotFound => Results.NotFound(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerifiedEmail => Results.Conflict(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            _ => Results.InternalServerError(new ResponseDto<string?>()
            {
                Data = null,
                Message = "Erro desconhecido ao reenviar verificação de e-mail.",
            })
        };

        return result;
    }

    private static IResult _BuildAccountCreationResponse(AccountDto newAccount)
    {
        return Results.Created(
            $"/accounts/{newAccount.UserType}/{newAccount.UserId}",
            new ResponseDto<AccountDto>
            {
                Data = newAccount,
                Message = "Parabéns! Sua conta foi criada com sucesso!"
            });
    }

    public static IResult _BuildAccountCreationFailureResponse(AuthBaseError error)
    {
        var result = error.ErrorType switch
        {
            ErrorType.InvalidAccountData => Results.BadRequest(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.AccountEmailNotFound => Results.NotFound(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.InvalidVerificationCode => Results.UnprocessableEntity(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationExpired => Results.UnprocessableEntity(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.EmailAlreadyRegistered => Results.Conflict(new ResponseDto<ErrorType>()
            {
                Data = error.ErrorType,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            _ => Results.InternalServerError(new ResponseDto<string?>()
            {
                Data = null,
                Message = "Erro desconhecido ao verificar e-mail.",
            })
        };

        return result;
    }
}
