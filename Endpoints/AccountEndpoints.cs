using Auth.Dtos;
using Auth.Repositories;

namespace Auth.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounts");

        group.MapPost("/send-verification", async (
            SendVerificationDto dto,
            IAccountRepository repository,
            CancellationToken ct) =>
        {
            try
            {
                var code = await repository.SendVerificationCodeAsync(dto.Email, ct);
                return Results.Ok(new { message = "Verification code sent.", code });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { ex.Message });
            }
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
}
