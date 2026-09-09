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

        group.MapGet("/{userType}/{userId:guid}", async (
            Guid userId,
            Enums.UserType userType,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            var account = await repository.GetByUserAsync(userId, userType, ct);
            return account is null ? Results.NotFound() : Results.Ok(account);
        });

        group.MapPost("/", async (NewAccountDto dto, IAccountRepository repository, CancellationToken ct) =>
        {
            try
            {
                var created = await repository.CreateAsync(dto, ct);
                return Results.Created($"/accounts/{created.UserType}/{created.UserId}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { ex.Message });
            }
        });

        group.MapPut("/{userType}/{userId:guid}", async (
            Guid userId,
            Enums.UserType userType,
            UpdateAccountDto dto,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            try
            {
                var updated = await repository.UpdateAsync(userId, userType, dto, ct);
                return Results.Ok(updated);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("/{userType}/{userId:guid}", async (
            Guid userId,
            Enums.UserType userType,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            var deleted = await repository.DeleteAsync(userId, userType, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
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
            ErrorType.ValidationError => Results.BadRequest(new ResponseDto<string?>()
            {
                Data = error.ErrorType.ToString(),
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerifiedEmail => Results.Conflict(new ResponseDto<string?>()
            {
                Data = error.ErrorType.ToString(),
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationExpired => Results.UnprocessableEntity(new ResponseDto<string?>()
            {
                Data = error.ErrorType.ToString(),
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationCodeSent => Results.UnprocessableEntity(new ResponseDto<string?>()
            {
                Data = error.ErrorType.ToString(),
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            _ => Results.InternalServerError(new ResponseDto<string?>()
            {
                Data = null,
                Message = error?.Message ?? "",
                Errors = error?.Errors ?? []
            }),
            ErrorType.VerificationExpired => Results.UnprocessableEntity(new ResponseDto<string?>()
            {
                Data = null,
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
