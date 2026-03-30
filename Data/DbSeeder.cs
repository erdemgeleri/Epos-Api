using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;
using WebApplication1;
using WebApplication1.Options;
using WebApplication1.Services;

namespace WebApplication1.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, SeedOptions seed, AppPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        var adminEmail = seed.AdminEmail.Trim().ToLowerInvariant();
        var businessEmail = seed.BusinessEmail.Trim().ToLowerInvariant();
        var customerEmail = seed.CustomerEmail.Trim().ToLowerInvariant();

        if (!await db.Businesses.AnyAsync(b => b.Id == DemoCredentials.DemoBusinessId, cancellationToken))
        {
            db.Businesses.Add(new Business
            {
                Id = DemoCredentials.DemoBusinessId,
                Name = seed.DemoBusinessName,
                Description = "Demo ortamÄ± iÃ§in Ã¶rnek iÅŸletme.",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        await UpsertDemoUserAsync(
            db,
            passwordHasher,
            DemoCredentials.DemoAdminUserId,
            adminEmail,
            seed.AdminDisplayName,
            UserRole.Admin,
            businessId: null,
            seed.AdminPassword,
            cancellationToken);

        await UpsertDemoUserAsync(
            db,
            passwordHasher,
            DemoCredentials.DemoBusinessUserId,
            businessEmail,
            seed.BusinessUserDisplayName,
            UserRole.Business,
            DemoCredentials.DemoBusinessId,
            seed.DemoPassword,
            cancellationToken);

        await UpsertDemoUserAsync(
            db,
            passwordHasher,
            DemoCredentials.DemoCustomerUserId,
            customerEmail,
            seed.CustomerDisplayName,
            UserRole.Customer,
            businessId: null,
            seed.DemoPassword,
            cancellationToken);
    }

    private static async Task UpsertDemoUserAsync(
        AppDbContext db,
        AppPasswordHasher passwordHasher,
        Guid desiredId,
        string email,
        string displayName,
        UserRole role,
        Guid? businessId,
        string password,
        CancellationToken cancellationToken)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (existing is not null)
        {
            existing.PasswordHash = passwordHasher.Hash(existing, password);
            existing.DisplayName = displayName;
            existing.Role = role;
            existing.BusinessId = businessId;
            existing.IsActive = true;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        var user = new User
        {
            Id = desiredId,
            Email = email,
            DisplayName = displayName,
            Role = role,
            BusinessId = businessId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            PasswordHash = string.Empty,
        };
        user.PasswordHash = passwordHasher.Hash(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
