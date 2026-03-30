namespace WebApplication1;

public static class DemoCredentials
{
    public const string AdminEmail = "admin@posdemo.local";
    public const string BusinessEmail = "business@posdemo.local";
    public const string CustomerEmail = "customer@posdemo.local";

    public const string Password = "PosDemo2026!";

    public const string AdminDisplayName = "Demo Admin";
    public const string BusinessUserDisplayName = "Demo Ä°ÅŸletme";
    public const string CustomerDisplayName = "Demo MÃ¼ÅŸteri";
    public const string DemoBusinessName = "Demo MaÄŸaza";

    public static readonly Guid DemoBusinessId = Guid.Parse("00000001-0000-0000-0000-000000000001");
    public static readonly Guid DemoAdminUserId = Guid.Parse("00000002-0000-0000-0000-000000000001");
    public static readonly Guid DemoBusinessUserId = Guid.Parse("00000003-0000-0000-0000-000000000001");
    public static readonly Guid DemoCustomerUserId = Guid.Parse("00000004-0000-0000-0000-000000000001");
}
