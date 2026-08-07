using Auth.Data;
using Auth.Dtos;
using Auth.Enums;
using Auth.Extensions;
using Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;

    public AccountRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AccountDto?> GetByUserAsync(Guid userId, UserType userType, CancellationToken cancellationToken = default)
    {
        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.UserType == userType, cancellationToken);

        return account?.MapToDto();
    }

    public async Task<string> SendVerificationCodeAsync(string email, CancellationToken cancellationToken = default)
    {
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

        _db.EmailVerifications.Add(verification);
        await _db.SaveChangesAsync(cancellationToken);

        return code;
    }

    public async Task<AccountDto> CreateAsync(NewAccountDto dto, CancellationToken cancellationToken = default)
    {
        var verification = await _db.EmailVerifications
            .Where(v => v.Email == dto.Email && v.Code == dto.VerificationCode && !v.Used && v.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (verification is null)
            throw new InvalidOperationException("Erro ao validar e-mail");

        verification.Used = true;

        var userId = Guid.NewGuid();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserType = dto.UserType,
            Email = dto.Email,
            Password = dto.Password,
            VerificationCode = dto.VerificationCode
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync(cancellationToken);

        return account.MapToDto();
    }

    public async Task<AccountDto> UpdateAsync(Guid userId, Enums.UserType userType, UpdateAccountDto dto, CancellationToken cancellationToken = default)
    {
        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.UserType == userType, cancellationToken);

        if (account is null)
            throw new InvalidOperationException($"Account for {userType} with id '{userId}' not found.");

        if (dto.Email is not null) account.Email = dto.Email;
        if (dto.Password is not null) account.Password = dto.Password;
        if (dto.VerificationCode is not null) account.VerificationCode = dto.VerificationCode;

        await _db.SaveChangesAsync(cancellationToken);

        return account.MapToDto();
    }

    public async Task<bool> DeleteAsync(Guid userId, Enums.UserType userType, CancellationToken cancellationToken = default)
    {
        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.UserType == userType, cancellationToken);

        if (account is null) return false;

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
