using WebApplication1;

namespace WebApplication1.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = DemoCredentials.AdminEmail;
    public string AdminPassword { get; set; } = DemoCredentials.Password;
    public string AdminDisplayName { get; set; } = DemoCredentials.AdminDisplayName;

    public string BusinessEmail { get; set; } = DemoCredentials.BusinessEmail;
    public string CustomerEmail { get; set; } = DemoCredentials.CustomerEmail;
    public string BusinessUserDisplayName { get; set; } = DemoCredentials.BusinessUserDisplayName;
    public string CustomerDisplayName { get; set; } = DemoCredentials.CustomerDisplayName;
    public string DemoBusinessName { get; set; } = DemoCredentials.DemoBusinessName;

    public string DemoPassword { get; set; } = DemoCredentials.Password;
}
