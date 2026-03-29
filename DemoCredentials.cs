namespace WebApplication1;

/// <summary>
/// Demo ortamı — e-posta ve şifreler (geliştirme). Üretimde kaldırın veya yapılandırmadan okuyun.
/// </summary>
public static class DemoCredentials
{
    public const string AdminEmail = "admin@posdemo.local";
    public const string BusinessEmail = "business@posdemo.local";
    public const string CustomerEmail = "customer@posdemo.local";

    /// <summary>Tüm demo hesaplar için ortak şifre.</summary>
    public const string Password = "PosDemo2026!";

    public const string AdminDisplayName = "Demo Admin";
    public const string BusinessUserDisplayName = "Demo İşletme";
    public const string CustomerDisplayName = "Demo Müşteri";
    public const string DemoBusinessName = "Demo Mağaza";

    public static readonly Guid DemoBusinessId = Guid.Parse("00000001-0000-0000-0000-000000000001");
    public static readonly Guid DemoAdminUserId = Guid.Parse("00000002-0000-0000-0000-000000000001");
    public static readonly Guid DemoBusinessUserId = Guid.Parse("00000003-0000-0000-0000-000000000001");
    public static readonly Guid DemoCustomerUserId = Guid.Parse("00000004-0000-0000-0000-000000000001");
}
