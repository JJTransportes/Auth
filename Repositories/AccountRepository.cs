using Auth.Data;
using Auth.Dtos;
using Auth.Enums;
using Auth.Errors;
using Auth.Errors.Account;
using Auth.Extensions;
using Auth.Interfaces;
using Auth.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories;

public class AccountRepository(
    AppDbContext db,
    IValidator<NewAccountDto> newAccountValidator) : IAccountRepository
{
    public async Task<Tuple<AccountDto?, AuthBaseError?>> CreateAsync(NewAccountDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = newAccountValidator.Validate(dto);
        if (!validationResult.IsValid)
        {
            var validationErros = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

            return Tuple.Create<AccountDto?, AuthBaseError?>(
                null,
                new AccountCreationError(
                    "Ops... Falha ao criar conta. Verifique os dados.",
                    validationErros,
                    ErrorType.InvalidAccountData
                    )
                );
        }

        var verification = await db.EmailVerifications
            .Where(emailVerification => emailVerification.Email == dto.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (verification is null)
        {
            return Tuple.Create<AccountDto?, AuthBaseError?>(
                null,
                new AccountCreationError(
                    "Ops... O e-mail informado não foi identificado.",
                    [],
                    ErrorType.AccountEmailNotFound
                    )
                );
        }

        if (verification.Code != dto.VerificationCode)
        {
            return Tuple.Create<AccountDto?, AuthBaseError?>(
                null,
                new AccountCreationError(
                    "Ops... O código de verificação informado não é válido.",
                    [],
                    ErrorType.InvalidVerificationCode
                    )
                );
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            return Tuple.Create<AccountDto?, AuthBaseError?>(
                null,
                new AccountCreationError(
                    "Ops... O prazo para verificação do e-mail expirou.",
                    [],
                    ErrorType.VerificationExpired
                    )
                );
        }

        var registeredAccount = await db.Accounts
            .Where(account => account.Email == dto.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (registeredAccount != null)
        {
            return Tuple.Create<AccountDto?, AuthBaseError?>(
               null,
               new AccountCreationError(
                   "Ops... Este e-mail já foi cadastrado.",
                   [],
                   ErrorType.EmailAlreadyRegistered
                   )
               );
        }

        verification.Used = true;

        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            UserType = dto.UserType,
            Email = dto.Email,
            Password = dto.Password,
            VerificationCode = dto.VerificationCode
        };

        db.Accounts.Add(newAccount);
        await db.SaveChangesAsync(cancellationToken);

        return Tuple.Create<AccountDto?, AuthBaseError?>(newAccount.MapToDto(), null);
    }

    public async Task<Tuple<AccountDto?, AuthBaseError?>> GetByUserAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account =>
                account.UserId == userId &&
                account.UserType == userType,
                cancellationToken
                );

        return Tuple.Create<AccountDto?, AuthBaseError?>(account?.MapToDto(), null);
    }

    public async Task<Tuple<AccountDto?, AuthBaseError?>> UpdateAsync(Guid userId, UserType userType, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(account =>
                account.UserId == userId &&
                account.UserType == userType,
                cancellationToken);

        if (account == null)
        {
            return Tuple.Create<AccountDto?, AuthBaseError?>(
                null,
                 new AccountCreationError(
                   "Ops... Este e-mail já foi cadastrado.",
                   [],
                   ErrorType.AccountNotFound
                   )
            );
        }

        if (dto.Email is not null) account.Email = dto.Email;
        if (dto.Password is not null) account.Password = dto.Password;
        if (dto.VerificationCode is not null) account.VerificationCode = dto.VerificationCode;

        await db.SaveChangesAsync(cancellationToken);

        return Tuple.Create<AccountDto?, AuthBaseError?>(account.MapToDto(), null);
    }

    public async Task<Tuple<bool, AuthBaseError?>> DeleteAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(account => account.UserId == userId && account.UserType == userType, cancellationToken);

        if (account == null)
        {
            return Tuple.Create<bool, AuthBaseError?>(
                false,
                 new AccountCreationError(
                   "Ops... Este e-mail já foi cadastrado.",
                   [],
                   ErrorType.AccountNotFound
                   )
            );
        }


        db.Accounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);

        return Tuple.Create<bool, AuthBaseError?>(true, null);
    }
}
