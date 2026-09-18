using Microsoft.AspNetCore.Identity;
using PMS.Application.Common.Interfaces;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Services;

/// <summary>
/// Thin wrapper around ASP.NET Core Identity's PasswordHasher (PBKDF2), used standalone since
/// the database schema (database/schema.sql) defines a simple `users.password_hash` column
/// rather than the full ASP.NET Identity table set.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string hash, string password) =>
        _inner.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
