using Microsoft.AspNetCore.Identity;
using WebApplication1.Entities;

namespace WebApplication1.Services;

public sealed class AppPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) =>
        _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) !=
        PasswordVerificationResult.Failed;
}
