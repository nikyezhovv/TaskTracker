using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TaskTracker.Api.Implementations.Model;
using TaskTracker.Api.Implementations.Repositories;
using TaskTracker.Api.Implementations.Repositories.Entities;

namespace TaskTracker.Api.Implementations.Services;

public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher<UserEntity> passwordHasher,
    IConfiguration config)
{
    public async Task<(Guid? Id, string? Error)> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var name = request.FullName.Trim();
        
        if (name.Length == 0) 
            return (null, "FullName is required.");
        
        if (await db.Users.AnyAsync(x => x.Email == email, ct))
            return (null, "Email is already registered.");

        var now = DateTimeOffset.UtcNow;
        var user = new UserEntity
        {
            Id = Guid.NewGuid(), Email = email, FullName = name,
            CreatedAt = now, UpdatedAt = now
        };
        
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return (null, "Email is already registered.");
        }
        
        return (user.Id, null);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        
        if (user is null) 
            return null;
        
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) 
            return null;
        
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        var response = IssueTokens(user);
        await db.SaveChangesAsync(ct);
        
        return response;
    }

    public async Task<AuthResponse?> RefreshAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) 
            return null;
        
        var hash = HashToken(rawToken);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var token = await db.RefreshTokens.AsNoTracking().SingleOrDefaultAsync(x => x.TokenHash == hash, ct);
        
        if (token is null || token.ExpiresAt <= DateTimeOffset.UtcNow || token.RevokedAt is not null)
            return null;
        
        var changed = await db.RefreshTokens
            .Where(x => x.Id == token.Id && 
                        x.RevokedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(s => 
                s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);
        
        if (changed != 1) 
            return null;
        
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == token.UserId, ct);
        if (user is null) 
            return null;
        
        var response = IssueTokens(user);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        
        return response;
    }

    public async Task LogoutAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken)) return;
        var hash = HashToken(rawToken);
        await db.RefreshTokens.Where(x => x.TokenHash == hash && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTimeOffset.UtcNow), ct);
    }

    private AuthResponse IssueTokens(UserEntity user)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Missing Jwt:Issuer");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Missing Jwt:Audience");
        var expires = DateTimeOffset.UtcNow.AddMinutes(15);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
        var jwt = new JwtSecurityToken(issuer, audience, claims,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        var access = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var now = DateTimeOffset.UtcNow;
        
        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            TokenHash = HashToken(refresh), CreatedAt = now,
            ExpiresAt = now.AddDays(30)
        });
        
        return new AuthResponse(access, refresh, expires);
    }

    private static string HashToken(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
}
